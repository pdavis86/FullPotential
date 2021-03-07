using UnityEngine;
using UnityEngine.Networking;

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
        UnityEngine.Networking.NetworkManager.singleton.StopClient();
        UnityEngine.Networking.NetworkManager.singleton.StopHost();
    }

    public void QuitGame()
    {
        GameManager.Quit();
    }

}
