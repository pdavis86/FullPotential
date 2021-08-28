using FullPotential.Assets.Core.Storage;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable ArrangeAccessorOwnerBody

public class GameManager : MonoBehaviour
{
    public const string NameCanvasScene = "SceneCanvas";
    public const string NameSpawnPoints = "SpawnPoints";

#pragma warning disable 0649
    [SerializeField] private GameObject _mainCanvas;
#pragma warning restore 0649

    //Core components
    public FullPotential.Assets.Core.Registry.TypeRegistry TypeRegistry { get; private set; }
    public FullPotential.Assets.Core.Localization.Localizer Localizer { get; private set; }
    public FullPotential.Assets.Core.Crafting.ResultFactory ResultFactory { get; private set; }


    //Behaviours
    public Prefabs Prefabs { get; private set; }
    public MainCanvasObjects MainCanvasObjects { get; private set; }


    //Properties
    public readonly GameManagerData DataStore = new GameManagerData();


    //Singleton
    private static GameManager _instance;
    public static GameManager Instance { get { return _instance; } }


    async void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        await UnityEngine.AddressableAssets.Addressables.InitializeAsync().Task;

        TypeRegistry = new FullPotential.Assets.Core.Registry.TypeRegistry();
        TypeRegistry.FindAndRegisterAll();

        var culture = GetLastUsedCulture();
        if (string.IsNullOrWhiteSpace(culture))
        {
            culture = FullPotential.Assets.Core.Localization.Localizer.DefaultCulture;
            SetLastUsedCulture(culture);
        }

        Localizer = new FullPotential.Assets.Core.Localization.Localizer(TypeRegistry.GetRegisteredModPaths());
        await Localizer.LoadAvailableCulturesAsync();
        await Localizer.LoadLocalizationFilesAsync(culture);

        ResultFactory = new FullPotential.Assets.Core.Crafting.ResultFactory(TypeRegistry, Localizer);

        Prefabs = GetComponent<Prefabs>();
        MainCanvasObjects = _mainCanvas.GetComponent<MainCanvasObjects>();

        MLAPI.NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;

        SceneManager.LoadSceneAsync(1);
    }

    private void ApprovalCheck(byte[] connectionData, ulong clientId, MLAPI.NetworkManager.ConnectionApprovedDelegate callback)
    {
        //Work-around for v0.1.0 of MLAPI not sending initial positions for GameObjects with Network Transform components
        //See https://github.com/Unity-Technologies/com.unity.netcode.gameobjects/issues/650
        GameObject.Find("TempEnemyShape").transform.position += new Vector3(1f, -1f, 1f);

        //todo: validate login credentials
        //todo: test to see if game is full
        //todo: test for a duplicate login

        callback(false, null, true, null, null);
    }

    private static string GetAppLoadOptions()
    {
        return System.IO.Path.Combine(Application.persistentDataPath, "LoadOptions.json");
    }

    public static void SetCulture(string culture)
    {
        Debug.Log("Setting language to " + culture);

        //todo: force all dialogue to reload content

        SetLastUsedCulture(culture);
    }

    private static void SetLastUsedCulture(string culture)
    {
        System.IO.File.WriteAllText(GetAppLoadOptions(), culture);
    }

    public static string GetLastUsedCulture()
    {
        var path = GetAppLoadOptions();

        if (!System.IO.File.Exists(path))
        {
            return null;
        }

        return System.IO.File.ReadAllText(path);
    }

    public static void Quit()
    {
        JoinOrHostGame.Disconnect();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit ();
#endif
    }

}
