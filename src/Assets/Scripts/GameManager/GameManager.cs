using Assets.Scripts.Crafting.Results;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

// ReSharper disable once CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

public class GameManager : MonoBehaviour
{
    public Inventory Inventory { get; private set; }

    public InputMappings InputMappings { get; private set; }

    public ResultFactory ResultFactory { get; private set; }


    // ReSharper disable once ArrangeAccessorOwnerBody
    private static GameManager _instance;
    public static GameManager Instance { get { return _instance; } }


    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
            Inventory = GetComponent<Inventory>();
            InputMappings = GetComponent<InputMappings>();
            ResultFactory = new ResultFactory();

            DontDestroyOnLoad(gameObject);
        }
    }

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
    }

    public static GameObject GetSceneObjects()
    {
        return UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects().FirstOrDefault(x => x.name == "SceneObjects");
    }

    public static GameObject GetCurrentPlayerGameObject(Camera playerCamera)
    {
        //var players = GameObject.FindGameObjectsWithTag("Player");
        return playerCamera.gameObject.transform.parent.gameObject;
    }

}
