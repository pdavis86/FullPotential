using Assets.Core.Crafting;
using Assets.Core.Registry;
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
    public Assets.Core.Localization.Localizer Localizer { get; private set; }
    public TypeRegistry TypeRegistry { get; private set; }
    public ResultFactory ResultFactory { get; private set; }

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

        //todo: get from settings
        Localizer = new Assets.Core.Localization.Localizer();
        var culture = "en-GB";
        var modPaths = new[] { "Standard/Localization" };
        Localizer.LoadLocalizationFiles(culture, modPaths);

        TypeRegistry = new Assets.Core.Registry.TypeRegistry();
        TypeRegistry.FindAndRegisterAll();

        ResultFactory = new ResultFactory(TypeRegistry, Localizer);

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
        //todo: username must be file name safe
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
