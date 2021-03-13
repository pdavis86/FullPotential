using Assets.Scripts.Crafting.Results;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable once CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

//todo: api for world creation
//todo: skin library website. By skin type, ordered by popularity

public class GameManager : MonoBehaviour
{
    //todo: move these
    public const string NameCanvasMain = "MainCanvas";
    public const string NameCanvasScene = "SceneCanvas";

    public MainCanvasObjects MainCanvasObjects { get; private set; }
    public Prefabs Prefabs { get; private set; }
    public InputMappings InputMappings { get; private set; }
    public ResultFactory ResultFactory { get; private set; }
    public string PlayerName { get; set; }
    public string PlayerSkinUrl { get; set; }
    public GameObject LocalPlayer { get; set; }


    // ReSharper disable once ArrangeAccessorOwnerBody
    private static GameManager _instance;
    public static GameManager Instance { get { return _instance; } }

    private Text _pingText;


    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        _instance = this;
        MainCanvasObjects = GameObject.Find(NameCanvasMain).GetComponent<MainCanvasObjects>();
        Prefabs = GetComponent<Prefabs>();
        InputMappings = GetComponent<InputMappings>();
        ResultFactory = new ResultFactory();

        //todo: do this better
        var pingGo = MainCanvasObjects.DebuggingOverlay.transform.Find("PingText");
        if (pingGo != null)
        {
            _pingText = pingGo.GetComponent<UnityEngine.UI.Text>();
        }

        DontDestroyOnLoad(gameObject);
    }

    //todo: mvoe this to a UI
    void OnGUI()
    {
        if (UnityEngine.Networking.NetworkClient.allClients.Count != 0)
        {
            if (_pingText != null)
            {
                var ping = UnityEngine.Networking.NetworkClient.allClients[0].GetRTT();
                _pingText.text = ping == 0 ? "Host" : ping + " ms";
            }
        }
        else
        {
            //textPing.text = "";

            //foreach (var netClient in UnityEngine.Networking.NetworkClient.allClients)
            //{
            //    textPing.text += netClient.connection.connectionId + " " + netClient.GetRTT() + " ms\n";
            //}

            //for (int i = 0; i < NetworkServer.connections.Count; ++i)
            //{
            //    var c = NetworkServer.connections[i];
            //    if (c == null || c.connectionId <= 0)
            //    {
            //        continue;
            //    }

            //    var rtt = NetworkTransport.GetCurrentRtt(c.hostId, c.connectionId, out var error);
            //    textPing.text += "Conn:" + c.connectionId + ", ping:" + rtt + " ms\n";
            //}
        }
    }

    //public static GameObject GetCurrentPlayerGameObject(Camera playerCamera)
    //{
    //    return playerCamera.gameObject.transform.parent.gameObject;
    //}

    public static void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit ();
#endif
    }








    //todo: move this
    public static GameObject GetObjectAtRoot(string name)
    {
        return UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects().FirstOrDefault(x => x.name == name);
    }

}
