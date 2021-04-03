using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Compiler

public class DebuggingUi : MonoBehaviour
{
    private Text _pingText;

    private void Awake()
    {
        var pingGo = GameManager.Instance.MainCanvasObjects.DebuggingOverlay.transform.Find("PingText");
        if (pingGo != null)
        {
            _pingText = pingGo.GetComponent<Text>();
        }
    }

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

}
