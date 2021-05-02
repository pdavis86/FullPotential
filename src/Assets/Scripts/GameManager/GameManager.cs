using UnityEngine;

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

    //Behaviours
    public MainCanvasObjects MainCanvasObjects { get; private set; }
    public Prefabs Prefabs { get; private set; }
    public InputMappings InputMappings { get; private set; }
    public GameObject LocalPlayer { get; set; }

    //Variables
    public string Username { get; set; }

    //Singleton
    private static GameManager _instance;
    public static GameManager Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        _instance = this;

        TypeRegistry = new Assets.Core.Registry.TypeRegistry();
        TypeRegistry.FindAndRegisterAll();

        //todo: get culture from settings
        var culture = "en-GB";

        Localizer = new Assets.Core.Localization.Localizer();
        Localizer.LoadLocalizationFiles(culture, TypeRegistry.GetRegisteredModPaths());

        ResultFactory = new Assets.Core.Crafting.ResultFactory(TypeRegistry, Localizer);

        MainCanvasObjects = GameObject.Find(NameCanvasMain).GetComponent<MainCanvasObjects>();
        Prefabs = GetComponent<Prefabs>();
        InputMappings = GetComponent<InputMappings>();

        DontDestroyOnLoad(gameObject);
    }

    //public static GameObject GetCurrentPlayerGameObject(Camera playerCamera)
    //{
    //    return playerCamera.gameObject.transform.parent.gameObject;
    //}

    public void SetPlayerUsername(string value)
    {
        Username = value;
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
