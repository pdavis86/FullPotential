using Assets.Scripts.GameManager.Models;
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
    public Assets.Core.Registry.TypeRegistry TypeRegistry { get; private set; }
    public Assets.Core.Localization.Localizer Localizer { get; private set; }
    public Assets.Core.Crafting.ResultFactory ResultFactory { get; private set; }


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

        TypeRegistry = new Assets.Core.Registry.TypeRegistry();
        TypeRegistry.FindAndRegisterAll();

        Localizer = new Assets.Core.Localization.Localizer(TypeRegistry.GetRegisteredModPaths());

        var culture = GetLastUsedCulture();
        if (string.IsNullOrWhiteSpace(culture))
        {
            culture = Localizer.GetAvailableCultureCodes().First();
            SetLastUsedCulture(culture);
        }

        await Localizer.LoadLocalizationFiles(culture);

        ResultFactory = new Assets.Core.Crafting.ResultFactory(TypeRegistry, Localizer);

        Prefabs = GetComponent<Prefabs>();
        MainCanvasObjects = _mainCanvas.GetComponent<MainCanvasObjects>();

        SceneManager.LoadSceneAsync(1);
    }

    private static string GetAppLoadOptions()
    {
        return System.IO.Path.Combine(Application.persistentDataPath, "LoadOptions.json");
    }

    public static void SetLastUsedCulture(string culture)
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
