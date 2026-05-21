using System;
using System.Collections;
using System.Linq;

using Cysharp.Threading.Tasks;

using FullPotential.Api.Data;
using FullPotential.Api.GameManagement.Models;
using FullPotential.Api.Ioc;
using FullPotential.Api.Localization;
using FullPotential.Api.Ui;
using FullPotential.Api.Utilities.Extensions;
using FullPotential.Core.Networking.Models;

using TMPro;

using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.GameManagement
{
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

        private IInstanceManagement _instanceManagement;
        private IUserManagement _userManagement;
        private ILocalizer _localizer;
        private IUiAssistant _uiAssistant;
        private ISettingsRepository _settingsRepository;
        private GameSettings _gameSettings;

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
            _instanceManagement = DependenciesContext.Dependencies.GetService<IInstanceManagement>();
            _userManagement = DependenciesContext.Dependencies.GetService<IUserManagement>();
            _localizer = DependenciesContext.Dependencies.GetService<ILocalizer>();
            _uiAssistant = DependenciesContext.Dependencies.GetService<IUiAssistant>();
            _settingsRepository = DependenciesContext.Dependencies.GetService<ISettingsRepository>();

            _gameSettings = _settingsRepository.Get();
            GameManager.Instance.LocalGameDataStore.PlayerToken = _gameSettings.LastSigninToken;
            _username = _gameSettings.LastSigninUsername;
        }

        // ReSharper disable once UnusedMember.Local
        private void Start()
        {
            _networkManager = NetworkManager.Singleton;
            _networkTransport = _networkManager.GetComponent<UnityTransport>();
            _onlineSceneName = System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(2));

            _networkManager.OnClientDisconnectCallback += OnClientDisconnect;
        }

        // ReSharper disable once UnusedMember.Local
#pragma warning disable UNT0006
        private async UniTask OnEnable()
        {
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
                await SignInWithTokenAsync();
            }

            ShowAnyError();
        }
#pragma warning restore UNT0006

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
        public void HandleUsernameAfterEdit(string value)
        {
            _username = value;
        }

        // ReSharper disable once UnusedMember.Global
        public void HandlePasswordAfterEdit(string value)
        {
            _password = value;
        }

        // ReSharper disable once UnusedMember.Global
        public void HandleNetworkAddressAfterEdit(string value)
        {
            _networkAddress = value;
        }

        // ReSharper disable once UnusedMember.Global
        public void HandleNetworkPortAfterEdit(string value)
        {
            _networkPort = value;
        }

        // ReSharper disable once UnusedMember.Global
        public void HandleHostAfterClick()
        {
            HostGameInternal();
        }

        // ReSharper disable once UnusedMember.Global
        public void HandleJoinAfterClick()
        {
            JoinGameInternal();
        }

        // ReSharper disable once UnusedMember.Global
        public void HandleQuitClick()
        {
            GameManager.Instance.Quit();
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public void HandleSignInClick()
        {
            if (_username.IsNullOrWhiteSpace())
            {
                _signinError.text = _localizer.Translate("ui.signin.missing");
                _signinError.gameObject.SetActive(true);
                return;
            }

            _signInContainer.SetActive(false);
            _signingInMessage.SetActive(true);

            SignInWithPasswordAsync().Forget();
        }

        public void HandleSignOutClick()
        {
            SignOutAsync().Forget();
        }

        #endregion

        private async UniTask SignInWithTokenAsync()
        {
            var isValid = await _userManagement.ValidateCredentialsAsync(_username, GameManager.Instance.LocalGameDataStore.PlayerToken);
            await HandleSignInResultAsync(GameManager.Instance.LocalGameDataStore.PlayerToken, !isValid);
        }

        private async UniTask SignInWithPasswordAsync()
        {
            var signInResult = await _userManagement.SignInWithPasswordAsync(_username, _password);
            await HandleSignInResultAsync(signInResult.Token, signInResult.IsInvalid);
        }

        private async UniTask HandleSignInResultAsync(string token, bool isInvalid = false)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                _signingInMessage.SetActive(false);

                _signinError.text = isInvalid
                    ? _localizer.Translate("ui.signin.invalid")
                    : _localizer.Translate("ui.signin.error");

                _signinError.gameObject.SetActive(true);
                _signInContainer.SetActive(true);
                return;
            }

            GameManager.Instance.LocalGameDataStore.PlayerToken = token;

            _gameSettings.LastSigninUsername = _username;
            _gameSettings.LastSigninToken = token;
            _settingsRepository.Save(_gameSettings);

            _signingInMessage.SetActive(false);
            _signinError.gameObject.SetActive(false);
            _signInContainer.SetActive(false);
            _username = _password = null;
            _signinUsername.text = _signinPassword.text = null;

            var connectionDetails = await _instanceManagement.GetConnectionDetailsAsync();

            if (connectionDetails != null)
            {
                _gameDetailsAddress.text = connectionDetails.Address;
                _gameDetailsPort.text = connectionDetails.Port.ToString();
            }
            else
            {
                _gameDetailsError.text = _localizer.Translate("ui.connect.nodetails");
                _gameDetailsError.gameObject.SetActive(true);
            }

            _gameDetailsContainer.SetActive(true);
            _gameDetailsAddress.Select();
        }

        // ReSharper disable once UnusedMember.Global
        public async UniTask SignOutAsync()
        {
            GameManager.Instance.LocalGameDataStore.PlayerToken = null;

            _gameDetailsContainer.SetActive(false);
            _signInContainer.SetActive(true);

            if (_signinUsername != null)
            {
                _username = _signinUsername.text;
                _signinUsername.Select();
            }

            await _userManagement.SignOutAsync();
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
                Username = _username,
                Token = GameManager.Instance.LocalGameDataStore.PlayerToken,
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

            JoinGameTimeoutAsync().Forget();
        }

        private async UniTask JoinGameTimeoutAsync()
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

                await UniTask.WaitForSeconds(1);

            } while (true);
        }
    }
}
