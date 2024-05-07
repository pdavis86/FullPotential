using System;
using System.Collections;
using System.Linq;
using FullPotential.Api.Ioc;
using FullPotential.Api.Localization;
using FullPotential.Api.Ui.Services;
using FullPotential.Api.Utilities.Extensions;
using FullPotential.Core.Networking.Data;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// ReSharper disable ClassNeverInstantiated.Global

//https://docs-multiplayer.unity3d.com/docs

namespace FullPotential.Core.GameManagement
{
    using FullPotential.Api.GameManagement;
    using FullPotential.Api.GameManagement.JsonModels;
    using TMPro;

    public class JoinOrHostGame : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField] private GameObject _signInContainer;
        [SerializeField] private InputField _signinUsername;
        [SerializeField] private TMP_InputField _signinPassword;
        [SerializeField] private Text _signinError;
        [SerializeField] private GameObject _gameDetailsContainer;
        [SerializeField] private InputField _gameDetailsAddress;
        [SerializeField] private InputField _gameDetailsPort;
        [SerializeField] private Text _gameDetailsError;
        [SerializeField] private GameObject _joiningMessage;
        [SerializeField] private GameObject _signingInMessage;
#pragma warning restore 0649

        // ReSharper disable MemberCanBePrivate.Global
        // ReSharper disable UnassignedField.Global
        public GameObject[] TabOrder;
        // ReSharper restore UnassignedField.Global
        // ReSharper restore MemberCanBePrivate.Global

        private IManagementService _managementService;
        private ILocalizer _localizer;
        private IUiAssistant _uiAssistant;

        private NetworkManager _networkManager;
        private UnityTransport _networkTransport;

        private string _onlineSceneName;
        private string _username;
        private string _password;
        private string _networkAddress;
        private string _networkPort;
        private DateTime _joinAttempt;
        private bool _shiftTab;

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            _managementService = DependenciesContext.Dependencies.GetService<IManagementService>();
            _localizer = DependenciesContext.Dependencies.GetService<ILocalizer>();
            _uiAssistant = DependenciesContext.Dependencies.GetService<IUiAssistant>();
        }

        // ReSharper disable once UnusedMember.Local
        private void Start()
        {
            _networkManager = NetworkManager.Singleton;
            _networkTransport = _networkManager.GetComponent<UnityTransport>();
            _onlineSceneName = System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(2));

            _networkManager.OnClientDisconnectCallback += OnClientDisconnect;

            _signinPassword.onSubmit.AddListener(_ => SignIn());
        }

        // ReSharper disable once UnusedMember.Local
        private void OnEnable()
        {
            _username = GameManager.Instance.GameSettings.LastSigninUsername;
            _signinUsername.text = _username;

            if (string.IsNullOrWhiteSpace(GameManager.Instance.LocalGameDataStore.PlayerToken))
            {
                _gameDetailsContainer.SetActive(false);
                _signInContainer.SetActive(true);
                if (_signinUsername != null)
                {
                    _signinUsername.Select();
                }
            }
            else
            {
                _signInContainer.SetActive(false);
                _gameDetailsContainer.SetActive(true);
                _gameDetailsAddress.Select();
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

        // ReSharper disable once UnusedMember.Local
        private void OnTabPress()
        {
            if (_shiftTab)
            {
                _shiftTab = false;
                return;
            }

            _uiAssistant.SelectNextGameObject(TabOrder, true);
        }

        // ReSharper disable once UnusedMember.Local
        private void OnShiftTabPress()
        {
            _shiftTab = true;

            _uiAssistant.SelectNextGameObject(TabOrder, false);
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

            _signInContainer.SetActive(false);
            _signingInMessage.SetActive(true);

            StartCoroutine(_managementService.SignInWithPasswordEnumerator(
                _username,
                _password,
                AfterSignIn,
                () => AfterSignIn(null)));
        }

        private void AfterSignIn(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                _signingInMessage.SetActive(false);

                _signinError.text = _localizer.Translate("ui.signin.error");
                _signinError.gameObject.SetActive(true);
                _signInContainer.SetActive(true);
                return;
            }

            _signinError.gameObject.SetActive(false);
            _signInContainer.SetActive(false);

            //_joiningMessage.SetActive(true);

            GameManager.Instance.GameSettings.LastSigninUsername = _username;
            GameManager.Instance.SaveGameSettings();

            GameManager.Instance.LocalGameDataStore.PlayerToken = token;
            _username = _password = null;

            StartCoroutine(_managementService.ConnectionDetailsCoroutine(
                AfterConnectionDetails,
                () => AfterConnectionDetails(null)));
        }

        private void AfterConnectionDetails(ConnectionDetails connectionDetails)
        {
            _signingInMessage.SetActive(false);

            //todo: check connectionDetails.Status
            // StartingUp = 0,
            // Available = 1,
            // Full = 2

            _gameDetailsAddress.text = connectionDetails.Address;
            _gameDetailsPort.text = connectionDetails.Port.ToString();

            _gameDetailsContainer.SetActive(true);
            _gameDetailsAddress.Select();
        }

        // ReSharper disable once UnusedMember.Global
        public void SignOut()
        {
            GameManager.Instance.LocalGameDataStore.PlayerToken = null;

            _gameDetailsContainer.SetActive(false);
            _signInContainer.SetActive(true);

            if (_signinUsername != null)
            {
                _username = _signinUsername.text;
                _signinUsername.Select();
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
                    Debug.LogWarning($"Server refused connection: {disconnectReason}");
                    _gameDetailsError.text = disconnectReason;
                }
                else
                {
                    _gameDetailsError.text = _localizer.Translate("ui.connect.disconnected");
                }

                _gameDetailsError.gameObject.SetActive(true);
            }
        }

        private void SetNetworkAddressAndPort()
        {
            var address = !string.IsNullOrWhiteSpace(_networkAddress)
                ? _networkAddress
                : "127.0.0.1";

            var desiredPort = ushort.TryParse(_networkPort, out var port)
                ? port
                : (ushort)7777;

            _networkTransport.SetConnectionData(address, desiredPort);
        }

        private void HostGameInternal()
        {
            _gameDetailsError.gameObject.SetActive(false);

            SetNetworkAddressAndPort();

            if (!IsPortFree(_networkTransport.ConnectionData.ListenEndPoint.Port))
            {
                _gameDetailsError.text = _localizer.Translate("ui.connect.portnotfree");
                _gameDetailsError.gameObject.SetActive(true);
                return;
            }

            GameManager.Instance.LocalGameDataStore.HasDisconnected = false;
            GameManager.Instance.ServerGameDataStore.ClientIdToUsername.Clear();

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
    }
}
