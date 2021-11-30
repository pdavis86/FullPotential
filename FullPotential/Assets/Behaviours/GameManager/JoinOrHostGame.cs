using FullPotential.Assets.Core.Data;
using Unity.Netcode;
using Unity.Netcode.Transports.UNET;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
    [SerializeField] private GameObject _signinError;
    [SerializeField] private GameObject _joiningMessage;
    [SerializeField] private Text _connectError;
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

        _networkManager.OnClientDisconnectCallback += OnClientDisconnect;
    }

    private void OnEnable()
    {
        if (string.IsNullOrWhiteSpace(GameManager.Instance.DataStore.PlayerToken))
        {
            _signInContainer.SetActive(true);
            _gameDetailsContainer.SetActive(false);
        }
        else
        {
            _signInContainer.SetActive(false);
            _gameDetailsContainer.SetActive(true);
        }

        ShowAnyError();
    }

    private void OnDisable()
    {
        if (_networkManager != null)
        {
            _networkManager.OnClientDisconnectCallback -= OnClientDisconnect;
        }
    }

    private void OnClientDisconnect(ulong clientId)
    {
        ShowAnyError();
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
            _signinError.SetActive(true);
            return;
        }

        GameManager.Instance.DataStore.PlayerToken = token;
        _username = _password = null;

        _signinError.SetActive(false);
        _signInContainer.SetActive(false);
        _gameDetailsContainer.SetActive(true);
    }

    private void ShowAnyError()
    {
        if (GameManager.Instance.DataStore.HasDisconnected && _connectError != null)
        {
            _gameDetailsContainer.SetActive(true);
            _joiningMessage.SetActive(false);

            _connectError.text = GameManager.Instance.Localizer.Translate("ui.connect.disconnected");
            _connectError.gameObject.SetActive(true);
        }
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
        _signinError.SetActive(false);

        SetNetworkAddressAndPort();

        if (!IsPortFree())
        {
            _connectError.text = GameManager.Instance.Localizer.Translate("ui.connect.portnotfree");
            _connectError.gameObject.SetActive(true);
            return;
        }

        GameManager.Instance.DataStore.HasDisconnected = false;

        _networkManager.StartHost();

        _gameDetailsContainer.SetActive(false);
        _joiningMessage.SetActive(true);

        NetworkManager.Singleton.SceneManager.LoadScene(_scene2Name, LoadSceneMode.Single);
    }

    private bool IsPortFree()
    {
        var ipEndpoints = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().GetActiveUdpListeners();

        foreach (var ipEndpoint in ipEndpoints)
        {
            if (ipEndpoint.Port == _networkTransport.ConnectPort)
            {
                return false;
            }
        }

        return true;
    }

    private void JoinGameInternal()
    {
        var payload = JsonUtility.ToJson(new ConnectionPayload()
        {
            PlayerToken = GameManager.Instance.DataStore.PlayerToken
        });
        NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.UTF8.GetBytes(payload);

        SetNetworkAddressAndPort();

        GameManager.Instance.DataStore.HasDisconnected = false;

        _networkManager.StartClient();

        _gameDetailsContainer.SetActive(false);
        _joiningMessage.SetActive(true);

        //NOTE: Do not need to change scene. This is handled by the server
    }

}
