using MLAPI;
using MLAPI.Transports.UNET;
using UnityEngine;
using UnityEngine.SceneManagement;

// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

//https://docs-multiplayer.unity3d.com/docs/getting-started/about-mlapi

//Known bugs with v0.1.0
//NetworkTransform doesn't sync data with new connected client https://github.com/Unity-Technologies/com.unity.netcode.gameobjects/issues/650
//Destroying an object via despawn leads to a warning message https://github.com/Unity-Technologies/com.unity.netcode.gameobjects/issues/677

public class JoinOrHostGame : MonoBehaviour
{
    private NetworkManager _networkManager;
    private UNetTransport _networkTransport;

    private string _scene2Name;
    private string _username;
    private string _password;
    private string _networkAddress;
    private string _networkPort;

    void Start()
    {
        _networkManager = NetworkManager.Singleton;
        _networkTransport = _networkManager.GetComponent<UNetTransport>();
        _scene2Name = System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(2));
    }

    #region Button Event Handlers

    public void SetPlayerUsername(string value)
    {
        _username = value;
    }

    public void SetPlayerPassword(string value)
    {
        _password = value;
    }

    public void SetNetworkAddress(string value)
    {
        _networkAddress = value;
    }

    public void SetNetworkPort(string value)
    {
        _networkPort = value;
    }

    public void HostGame()
    {
        HostGameInternal();
    }

    public void JoinGame()
    {
        JoinGameInternal();
    }

    public void QuitGame()
    {
        GameManager.Quit();
    }

    #endregion

    private void SignIn()
    {
        //todo: make a server-side call to sign in
        GameManager.Instance.DataStore.PlayerToken = Assets.Core.Registry.UserRegistry.SignIn(_username, _password);

        _username = _password = null;
    }

    private void SetNetworkAddressAndPort()
    {
        _networkTransport.ConnectAddress = !string.IsNullOrWhiteSpace(_networkAddress)
            ? _networkAddress
            : "127.0.0.1";

        _networkTransport.ConnectPort = int.TryParse(_networkPort, out var port)
            ? port
            : 7777;
    }

    private void HostGameInternal()
    {
        SignIn();

        SetNetworkAddressAndPort();
        _networkManager.StartHost();

        //todo: startResult is useless for findint out if we actualyl connected correctly

        MLAPI.SceneManagement.NetworkSceneManager.SwitchScene(_scene2Name);
    }

    private void JoinGameInternal()
    {
        SignIn();

        SetNetworkAddressAndPort();
        _networkManager.StartClient();

        //todo: startResult is useless for findint out if we actualyl connected correctly

        //NOTE: Do not need to change scene. This is handled by the server
    }

    public static void Disconnect()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.StopHost();
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            NetworkManager.Singleton.StopClient();
        }
    }

}
