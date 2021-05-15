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

    private string _username;
    private string _password;
    private string _networkAddress;
    private string _networkPort;

    void Start()
    {
        _networkManager = NetworkManager.singleton;
    }

    public void SetPlayerUsername(string value)
    {
        _username = value;
    }

    public void SetPlayerPassword(string value)
    {
        _password = value;
    }

    public void SignIn()
    {
        GameManager.Instance.UserRegistry.SignIn(_username, _password);
        _username = _password = null;
    }

    private void SetNetworkAddressAndPort()
    {
        _networkManager.networkAddress = !string.IsNullOrWhiteSpace(_networkAddress)
            ? _networkAddress
            : "localhost";

        if (!int.TryParse(_networkPort, out var port))
        {
            port = 7777;
        }
        _networkManager.networkPort = port;
    }

    public void HostGame()
    {
        SetNetworkAddressAndPort();
        _networkManager.StartHost();
    }

    public void JoinGame()
    {
        SignIn();
        SetNetworkAddressAndPort();
        _networkManager.StartClient();
    }

    public void SetNetworkAddress(string value)
    {
        _networkAddress = value;
    }

    public void SetNetworkPort(string value)
    {
        _networkPort = value;
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
