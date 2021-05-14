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
    public const string NameCanvasMain = "MainCanvas";
    public const string NameCanvasScene = "SceneCanvas";


    //Core components
    public Assets.Core.Registry.TypeRegistry TypeRegistry { get; private set; }
    public Assets.Core.Localization.Localizer Localizer { get; private set; }
    public Assets.Core.Crafting.ResultFactory ResultFactory { get; private set; }
    public Assets.Core.Registry.UserRegistry UserRegistry { get; private set; }


    //Behaviours
    public MainCanvasObjects MainCanvasObjects { get; private set; }
    public Prefabs Prefabs { get; private set; }


    //Variables
    public GameObject LocalPlayer { get; set; }


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

        //todo: get culture from settings
        var culture = "en-GB"; // ?? Localizer.GetAvailableCultures().First();

        await Localizer.LoadLocalizationFiles(culture);

        ResultFactory = new Assets.Core.Crafting.ResultFactory(TypeRegistry, Localizer);

        UserRegistry = new Assets.Core.Registry.UserRegistry();

        MainCanvasObjects = GameObject.Find(NameCanvasMain).GetComponent<MainCanvasObjects>();
        Prefabs = GetComponent<Prefabs>();

        SceneManager.LoadSceneAsync(1);
    }

    public static void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit ();
#endif
    }

}
