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
#pragma warning disable 0649
    [SerializeField] private GameObject _signInContainer;
    [SerializeField] private GameObject _gameDetailsContainer;
#pragma warning restore 0649

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

    public void SignIn()
    {
        var token = FullPotential.Assets.Core.Registry.UserRegistry.SignIn(_username, _password);

        if (string.IsNullOrWhiteSpace(token))
        {
            //todo: handle login failure
        }

        GameManager.Instance.DataStore.PlayerToken = token;
        _username = _password = null;
        _signInContainer.SetActive(false);
        _gameDetailsContainer.SetActive(true);
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
        SetNetworkAddressAndPort();
        _networkManager.StartHost();

        //todo: startResult is useless for findint out if we actualyl connected correctly

        MLAPI.SceneManagement.NetworkSceneManager.SwitchScene(_scene2Name);
    }

    private void JoinGameInternal()
    {
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
