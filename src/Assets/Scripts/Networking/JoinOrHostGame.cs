using UnityEngine;
using UnityEngine.Networking;

// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

public class JoinOrHostGame : MonoBehaviour
{
    private NetworkManager _networkManager;

    void Start()
    {
        _networkManager = NetworkManager.singleton;
    }

    public void HostGame()
    {
        _networkManager.networkAddress = "localhost";
        _networkManager.networkPort = 7777;
        _networkManager.StartHost();
    }

    public void JoinGame()
    {
        _networkManager.networkAddress = "localhost";
        _networkManager.networkPort = 7777;
        _networkManager.StartClient();
    }

    public void SetPlayerName(string value)
    {
       GameManager.Instance.PlayerName = value;
    }

    public void SetPlayerSkinUrl(string value)
    {
        GameManager.Instance.PlayerSkinUrl = value;
    }

    public static void Disconnect()
    {
        NetworkManager.singleton.StopClient();
        NetworkManager.singleton.StopHost();
    }

    public void QuitGame()
    {
        GameManager.Quit();
    }

}
