using Assets.Core;
using Assets.Core.Crafting;
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

    public MainCanvasObjects MainCanvasObjects { get; private set; }
    public Prefabs Prefabs { get; private set; }
    public InputMappings InputMappings { get; private set; }
    public ResultFactory ResultFactory { get; private set; }
    public string Username { get; set; }
    public GameObject LocalPlayer { get; set; }

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
        var culture = "en-GB";
        var modPaths = new[] { "Standard/Localization" };
        Assets.Core.Localization.Localizer.Instance.LoadLocalizationFiles(culture, modPaths);

        MainCanvasObjects = GameObject.Find(NameCanvasMain).GetComponent<MainCanvasObjects>();
        Prefabs = GetComponent<Prefabs>();
        InputMappings = GetComponent<InputMappings>();

        ApiRegister.Instance.FindAndRegisterAll();
        ResultFactory = new ResultFactory();

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
