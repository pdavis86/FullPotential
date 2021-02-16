using Assets.Scripts.Crafting.Results;
using System.Linq;
using UnityEngine;

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
    public MainCanvasObjects MainCanvasObjects { get; private set; }
    public Prefabs Prefabs { get; private set; }
    public InputMappings InputMappings { get; private set; }
    public ResultFactory ResultFactory { get; private set; }
    public string PlayerName { get; set; }
    public string PlayerSkinUrl { get; set; }


    // ReSharper disable once ArrangeAccessorOwnerBody
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
        MainCanvasObjects = GetMainCanvasObjects();
        Prefabs = GetComponent<Prefabs>();
        InputMappings = GetComponent<InputMappings>();
        ResultFactory = new ResultFactory();

        DontDestroyOnLoad(gameObject);
    }

    //private void Start()
    //{
    //    //Doesn't work
    //    //UnityEngine.SceneManagement.SceneManager.LoadScene("Offline");
    //}

    void OnGUI()
    {
        if (UnityEngine.Networking.NetworkClient.allClients.Count != 0)
        {
            var goPing = GameObject.Find("PingText");
            if (goPing != null)
            {
                var textPing = goPing.GetComponent<UnityEngine.UI.Text>();

                var ping = UnityEngine.Networking.NetworkClient.allClients[0].GetRTT();
                textPing.text = ping == 0 ? "Host" : ping + " ms";
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

    public static MainCanvasObjects GetMainCanvasObjects()
    {
        return GameObject.Find("MainCanvas").GetComponent<MainCanvasObjects>();
    }

    public static GameObject GetSceneCanvas()
    {
        return UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects().FirstOrDefault(x => x.name == "SceneCanvas");
    }

    public static GameObject GetCurrentPlayerGameObject(Camera playerCamera)
    {
        //var players = GameObject.FindGameObjectsWithTag("Player");
        return playerCamera.gameObject.transform.parent.gameObject;
    }



    public static void Disconnect()
    {
        UnityEngine.Networking.NetworkManager.singleton.StopClient();
        UnityEngine.Networking.NetworkManager.singleton.StopHost();
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
