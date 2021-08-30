using FullPotential.Assets.Core.Crafting;
using FullPotential.Assets.Core.Data;
using FullPotential.Assets.Core.Localization;
using FullPotential.Assets.Core.Registry;
using FullPotential.Assets.Core.Storage;
using MLAPI;
using System.Threading.Tasks;
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
    public TypeRegistry TypeRegistry { get; private set; }
    public Localizer Localizer { get; private set; }
    public ResultFactory ResultFactory { get; private set; }


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

        TypeRegistry = new TypeRegistry();
        TypeRegistry.FindAndRegisterAll();

        var culture = GetLastUsedCulture();
        if (string.IsNullOrWhiteSpace(culture))
        {
            culture = Localizer.DefaultCulture;
            SetLastUsedCulture(culture);
        }

        Localizer = new Localizer(TypeRegistry.GetRegisteredModPaths());
        await Localizer.LoadAvailableCulturesAsync();
        await Localizer.LoadLocalizationFilesAsync(culture);

        ResultFactory = new ResultFactory(TypeRegistry, Localizer);

        Prefabs = GetComponent<Prefabs>();
        MainCanvasObjects = _mainCanvas.GetComponent<MainCanvasObjects>();

        NetworkManager.Singleton.ConnectionApprovalCallback += OnApprovalCheck;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;

        SceneManager.LoadSceneAsync(1);
    }

    private void OnApprovalCheck(byte[] connectionData, ulong clientId, NetworkManager.ConnectionApprovedDelegate callback)
    {
        //Work-around for v0.1.0 of MLAPI not sending initial positions for GameObjects with Network Transform components
        //See https://github.com/Unity-Technologies/com.unity.netcode.gameobjects/issues/650
        GameObject.Find("TempEnemyShape").transform.position += new Vector3(0f, -1f, 0f);

        var payload = System.Text.Encoding.UTF8.GetString(connectionData);
        var connectionPayload = JsonUtility.FromJson<ConnectionPayload>(payload);
        if (!UserRegistry.ValidateToken(connectionPayload.PlayerToken))
        {
            Debug.LogWarning("Someone tried to connect with an invalid Player token");
            callback(false, null, false, null, null);
            return;
        }

        callback(false, null, true, null, null);
    }

    private void OnClientDisconnect(ulong clientId)
    {
        Debug.LogWarning("Disconnected from server");

        GameManager.Instance.DataStore.HasDisconnected = true;

        if (SceneManager.GetActiveScene().buildIndex != 1)
        {
            SceneManager.LoadSceneAsync(1);
        }
    }

    // ReSharper disable once UnusedMethodReturnValue.Global
    public async Task<bool> SetCultureAsync(string culture)
    {
        Debug.Log("Setting language to " + culture);

        await Localizer.LoadLocalizationFilesAsync(culture);

        //Re-activate anything already active
        MainCanvasObjects.DebuggingOverlay.SetActive(false);
        MainCanvasObjects.DebuggingOverlay.SetActive(true);

        SetLastUsedCulture(culture);

        return true;
    }

    private static string GetAppOptionsPath()
    {
        return System.IO.Path.Combine(Application.persistentDataPath, "LoadOptions.json");
    }

    private static void SetLastUsedCulture(string culture)
    {
        var options = new AppOptions
        {
            Culture = culture
        };

        System.IO.File.WriteAllText(GetAppOptionsPath(), JsonUtility.ToJson(options));
    }

    public static string GetLastUsedCulture()
    {
        var path = GetAppOptionsPath();

        if (!System.IO.File.Exists(path))
        {
            return null;
        }

        var options = JsonUtility.FromJson<AppOptions>(System.IO.File.ReadAllText(path));

        return options.Culture;
    }

    public static void Disconnect()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.StopHost();
            SceneManager.LoadSceneAsync(1);
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            NetworkManager.Singleton.StopClient();
        }
    }

    public static void Quit()
    {
        //todo: necessary? - Disconnect();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit ();
#endif
    }

}
