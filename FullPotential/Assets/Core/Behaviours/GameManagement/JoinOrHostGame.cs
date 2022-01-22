using System;
using System.Collections;
using System.Linq;
using FullPotential.Core.Data;
using FullPotential.Core.Extensions;
using Unity.Netcode;
using Unity.Netcode.Transports.UNET;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// ReSharper disable UnusedMember.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

//https://docs-multiplayer.unity3d.com/docs

namespace FullPotential.Core.Behaviours.GameManagement
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

        private NetworkManager _networkManager;
        private UNetTransport _networkTransport;

        private string _scene2Name;
        private string _username;
        private string _password;
        private string _networkAddress;
        private string _networkPort;
        private DateTime _joinAttempt;

        // ReSharper disable once UnusedMember.Local
        private void Start()
        {
            _networkManager = NetworkManager.Singleton;
            _networkTransport = _networkManager.GetComponent<UNetTransport>();
            _scene2Name = System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(2));

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
            GameManager.Instance.Quit();
        }

        #endregion

        public void SignIn()
        {
            if (_username.IsNullOrWhiteSpace())
            {
                _signinError.text = GameManager.Instance.Localizer.Translate("ui.signin.missing");
                _signinError.gameObject.SetActive(true);
                return;
            }

            var token = GameManager.Instance.UserRegistry.SignIn(_username, _password);

            if (string.IsNullOrWhiteSpace(token))
            {
                _signinError.text = GameManager.Instance.Localizer.Translate("ui.signin.error");
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
            if (GameManager.Instance.LocalGameDataStore.HasDisconnected)
            {
                _gameDetailsContainer.SetActive(true);
                _joiningMessage.SetActive(false);

                _gameDetailsError.text = GameManager.Instance.Localizer.Translate("ui.connect.disconnected");
                _gameDetailsError.gameObject.SetActive(true);
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
            _signinError.gameObject.SetActive(false);

            SetNetworkAddressAndPort();

            if (!IsPortFree())
            {
                _signinError.text = GameManager.Instance.Localizer.Translate("ui.connect.portnotfree");
                _signinError.gameObject.SetActive(true);
                return;
            }

            GameManager.Instance.LocalGameDataStore.HasDisconnected = false;

            _networkManager.StartHost();

            _gameDetailsContainer.SetActive(false);
            _joiningMessage.SetActive(true);

            NetworkManager.Singleton.SceneManager.LoadScene(_scene2Name, LoadSceneMode.Single);
        }

        private bool IsPortFree()
        {
            var ipEndpoints = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().GetActiveUdpListeners();
            return ipEndpoints.All(ipEndpoint => ipEndpoint.Port != _networkTransport.ConnectPort);
        }

        private void JoinGameInternal()
        {
            var payload = JsonUtility.ToJson(new ConnectionPayload
            {
                PlayerToken = GameManager.Instance.LocalGameDataStore.PlayerToken
            });
            NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.UTF8.GetBytes(payload);

            SetNetworkAddressAndPort();

            GameManager.Instance.LocalGameDataStore.HasDisconnected = false;

            _joinAttempt = DateTime.UtcNow;
            _networkManager.StartClient();

            _gameDetailsContainer.SetActive(false);
            _joiningMessage.SetActive(true);

            //NOTE: Do not need to change scene. This is handled by the server

            StartCoroutine(LogTimeout());
        }

        private IEnumerator LogTimeout()
        {
            const int timeoutSeconds = 10;

            do
            {
                var timeTaken = (DateTime.UtcNow - _joinAttempt).TotalSeconds;
                if (timeTaken > timeoutSeconds)
                {
                    NetworkManager.Singleton.Shutdown();

                    Debug.LogWarning($"Failed to join game after {timeoutSeconds} seconds");
                    _signinError.text = GameManager.Instance.Localizer.Translate("ui.connect.jointimeout");
                    _signinError.gameObject.SetActive(true);
                    _joiningMessage.SetActive(false);
                    _gameDetailsContainer.SetActive(true);
                    break;
                }

                yield return new WaitForSeconds(1);

            } while (true);
        }

    }
}
