using System;
using System.Collections;
using System.Linq;
using FullPotential.Api.Ioc;
using FullPotential.Api.Localization;
using FullPotential.Api.Registry;
using FullPotential.Api.Utilities.Extensions;
using FullPotential.Core.GameManagement.Enums;
using FullPotential.Core.Networking.Data;
using Unity.Netcode;
using Unity.Netcode.Transports.UNET;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// ReSharper disable ClassNeverInstantiated.Global

//https://docs-multiplayer.unity3d.com/docs

namespace FullPotential.Core.GameManagement
{
    public class JoinOrHostGame : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField] private GameObject _signInContainer;
        [SerializeField] private InputField _signinFirstInput;
        [SerializeField] private Text _signinError;
        [SerializeField] private GameObject _gameDetailsContainer;
        [SerializeField] private InputField _gameDetailsFirstInput;
        [SerializeField] private Text _gameDetailsError;
        [SerializeField] private GameObject _joiningMessage;
#pragma warning restore 0649

        private IUserRegistry _userRegistry;
        private ILocalizer _localizer;

        private NetworkManager _networkManager;
        private UNetTransport _networkTransport;

        private string _onlineSceneName;
        private string _username;
        private string _password;
        private string _networkAddress;
        private string _networkPort;
        private DateTime _joinAttempt;

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            _userRegistry = DependenciesContext.Dependencies.GetService<IUserRegistry>();
            _localizer = DependenciesContext.Dependencies.GetService<ILocalizer>();
        }

        // ReSharper disable once UnusedMember.Local
        private void Start()
        {
            _networkManager = NetworkManager.Singleton;
            _networkTransport = _networkManager.GetComponent<UNetTransport>();
            _onlineSceneName = System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(2));

            _networkManager.OnClientDisconnectCallback += OnClientDisconnect;
        }

        // ReSharper disable once UnusedMember.Local
        private void OnEnable()
        {
            _username = GameManager.Instance.AppOptions.Username;
            _signinFirstInput.text = _username;

            if (string.IsNullOrWhiteSpace(GameManager.Instance.LocalGameDataStore.PlayerToken))
            {
                _gameDetailsContainer.SetActive(false);
                _signInContainer.SetActive(true);
                if (_signinFirstInput != null)
                {
                    _signinFirstInput.Select();
                }
            }
            else
            {
                _signInContainer.SetActive(false);
                _gameDetailsContainer.SetActive(true);
                if (_gameDetailsFirstInput != null)
                {
                    _gameDetailsFirstInput.Select();
                }
            }

            ShowAnyError();
        }

        // ReSharper disable once UnusedMember.Local
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

        // ReSharper disable once UnusedMember.Global
        public void SetPlayerUsername(string value)
        {
            _username = value;
        }

        // ReSharper disable once UnusedMember.Global
        public void SetPlayerPassword(string value)
        {
            _password = value;
        }

        // ReSharper disable once UnusedMember.Global
        public void SetNetworkAddress(string value)
        {
            _networkAddress = value;
        }

        // ReSharper disable once UnusedMember.Global
        public void SetNetworkPort(string value)
        {
            _networkPort = value;
        }

        // ReSharper disable once UnusedMember.Global
        public void HostGame()
        {
            HostGameInternal();
        }

        // ReSharper disable once UnusedMember.Global
        public void JoinGame()
        {
            JoinGameInternal();
        }

        // ReSharper disable once UnusedMember.Global
        public void QuitGame()
        {
            GameManager.Instance.Quit();
        }

        #endregion

        // ReSharper disable once UnusedMember.Global
        public void SignIn()
        {
            if (_username.IsNullOrWhiteSpace())
            {
                _signinError.text = _localizer.Translate("ui.signin.missing");
                _signinError.gameObject.SetActive(true);
                return;
            }

            var token = _userRegistry.SignIn(_username, _password);

            if (string.IsNullOrWhiteSpace(token))
            {
                _signinError.text = _localizer.Translate("ui.signin.error");
                _signinError.gameObject.SetActive(true);
                return;
            }

            GameManager.Instance.AppOptions.Username = _username;

            GameManager.Instance.LocalGameDataStore.PlayerToken = token;
            _username = _password = null;

            _signinError.gameObject.SetActive(false);
            _signInContainer.SetActive(false);
            _gameDetailsContainer.SetActive(true);
            if (_gameDetailsFirstInput != null)
            {
                _gameDetailsFirstInput.Select();
            }
        }

        private void ShowAnyError()
        {
            if (GameManager.Instance.LocalGameDataStore.HasDisconnected && !_gameDetailsError.gameObject.activeInHierarchy)
            {
                _gameDetailsContainer.SetActive(true);
                _joiningMessage.SetActive(false);

                var disconnectReason = GameManager.Instance.LocalGameDataStore.DisconnectReason;

                if (!string.IsNullOrWhiteSpace(disconnectReason))
                {
                    Debug.LogWarning($"Server refused connection with status {disconnectReason}");
                    _gameDetailsError.text = string.Format(_localizer.Translate("ui.connect.joinrejected"), disconnectReason);
                }
                else
                {
                    _gameDetailsError.text = _localizer.Translate("ui.connect.disconnected");
                }

                _gameDetailsError.gameObject.SetActive(true);
            }
        }

        private void SetNetworkAddressAndPort(bool isHosting = false)
        {
            _networkTransport.ConnectAddress = !string.IsNullOrWhiteSpace(_networkAddress)
                ? _networkAddress
                : "127.0.0.1";

            var desiredPort = int.TryParse(_networkPort, out var port)
                ? port
                : 7777;

            if (isHosting)
            {
                _networkTransport.ServerListenPort = desiredPort;
            }
            else
            {
                _networkTransport.ConnectPort = desiredPort;
            }
            
        }

        private void HostGameInternal()
        {
            _gameDetailsError.gameObject.SetActive(false);

            SetNetworkAddressAndPort(true);

            if (!IsPortFree(_networkTransport.ServerListenPort))
            {
                _gameDetailsError.text = _localizer.Translate("ui.connect.portnotfree");
                _gameDetailsError.gameObject.SetActive(true);
                return;
            }

            GameManager.Instance.LocalGameDataStore.HasDisconnected = false;
            GameManager.Instance.GameDataStore.ClientIdToUsername.Clear();

            _networkManager.StartHost();

            _gameDetailsContainer.SetActive(false);
            _joiningMessage.SetActive(true);

            NetworkManager.Singleton.SceneManager.LoadScene(_onlineSceneName, LoadSceneMode.Single);
        }

        private bool IsPortFree(int port)
        {
            var ipEndpoints = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().GetActiveUdpListeners();
            return ipEndpoints.All(ipEndpoint => ipEndpoint.Port != port);
        }

        private void JoinGameInternal()
        {
            var payload = JsonUtility.ToJson(new ConnectionPayload
            {
                PlayerToken = GameManager.Instance.LocalGameDataStore.PlayerToken,
                GameVersion = GameManager.GetGameVersion().ToString()
            });
            NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.UTF8.GetBytes(payload);

            SetNetworkAddressAndPort();

            GameManager.Instance.LocalGameDataStore.HasDisconnected = false;

            _joinAttempt = DateTime.UtcNow;
            _networkManager.StartClient();

            _networkManager.CustomMessagingManager.UnregisterNamedMessageHandler(nameof(SetDisconnectReasonClientCustomMessage));
            _networkManager.CustomMessagingManager.RegisterNamedMessageHandler(nameof(SetDisconnectReasonClientCustomMessage), SetDisconnectReasonClientCustomMessage);

            _gameDetailsContainer.SetActive(false);
            _joiningMessage.SetActive(true);

            //NOTE: Do not need to change scene. This is handled by the server

            StartCoroutine(JoinGameTimeout());
        }

        private IEnumerator JoinGameTimeout()
        {
            const int timeoutSeconds = 10;

            do
            {
                var timeTaken = (DateTime.UtcNow - _joinAttempt).TotalSeconds;
                if (timeTaken > timeoutSeconds)
                {
                    NetworkManager.Singleton.Shutdown();

                    Debug.LogWarning($"Failed to join game after {timeoutSeconds} seconds");

                    if (!_gameDetailsError.gameObject.activeInHierarchy)
                    {
                        _gameDetailsError.text = _localizer.Translate("ui.connect.jointimeout");
                        _gameDetailsError.gameObject.SetActive(true);
                        _joiningMessage.SetActive(false);
                        _gameDetailsContainer.SetActive(true);
                    }

                    break;
                }

                yield return new WaitForSeconds(1);

            } while (true);
        }

        public void SetDisconnectReasonClientCustomMessage(ulong clientId, FastBufferReader reader)
        {
            reader.ReadValueSafe(out ConnectStatus status);

            Debug.LogWarning($"Server refused connection with status {status}");
            
            GameManager.Instance.LocalGameDataStore.DisconnectReason = status.ToString();
        }

    }
}
