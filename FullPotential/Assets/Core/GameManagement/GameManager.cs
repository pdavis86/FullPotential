using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Cysharp.Threading.Tasks;

using FullPotential.Api.Data;
using FullPotential.Api.GameManagement;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Combat.Events;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Gameplay.Inventory.Events;
using FullPotential.Api.Gameplay.Player;
using FullPotential.Api.Ioc;
using FullPotential.Api.Localization;
using FullPotential.Api.Registry;
using FullPotential.Api.Scenes;
using FullPotential.Api.Ui;
using FullPotential.Api.Unity;
using FullPotential.Api.Unity.Constants;
using FullPotential.Api.Utilities;
using FullPotential.Core.GameManagement.Data;
using FullPotential.Core.Gameplay.Events;
using FullPotential.Core.Networking.Models;
using FullPotential.Core.Player;
using FullPotential.Core.Registry;

using Unity.Netcode;

using UnityEngine;
using UnityEngine.SceneManagement;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.GameManagement
{
    public class GameManager : MonoBehaviour, IGameManager
    {
        //Editor properties
#pragma warning disable 0649
        [SerializeField] private GameObject _mainCanvas;
#pragma warning restore 0649

        //Non-editor properties
        public Prefabs Prefabs { get; private set; }
        public UserInterface UserInterface { get; private set; }

        //Input
        public DefaultInputActions InputActions { get; private set; }

        //Data Stores
        public readonly ServerGameData ServerGameDataStore = new ServerGameData();
        public readonly LocalGameData LocalGameDataStore = new LocalGameData();

        //Services
        private ISettingsRepository _settingsRepository;
        private ISaveManager _saveManager;
        private IUserManagement _userManagement;
        private ILocalizer _localizer;
        private IUnityHelperUtilities _unityHelperUtilities;

        //Variables
        private NetworkObject _playerPrefabNetObj;
        private DelayedAction _periodicSave;
        private bool _serverHasBeenStarted;
        private Transform _playersParentTransform;

        //Singleton
        public static GameManager Instance { get; private set; }

        #region Unity Event Handlers

        // ReSharper disable once UnusedMember.Local
#pragma warning disable UNT0006 // Incorrect message signature
        private async Task Awake()
#pragma warning restore UNT0006 // Incorrect message signature
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            Prefabs = GetComponent<Prefabs>();
            UserInterface = _mainCanvas.GetComponent<UserInterface>();

            ServiceManager.RegisterServices();

            _settingsRepository = DependenciesContext.Dependencies.GetService<ISettingsRepository>();
            _saveManager = DependenciesContext.Dependencies.GetService<ISaveManager>();
            _userManagement = DependenciesContext.Dependencies.GetService<IUserManagement>();
            _localizer = DependenciesContext.Dependencies.GetService<ILocalizer>();
            _unityHelperUtilities = DependenciesContext.Dependencies.GetService<IUnityHelperUtilities>();

            RegisterEvents();

            await UnityEngine.AddressableAssets.Addressables.InitializeAsync().Task;

            var addressablesManager = new AddressablesManager();

            var typeRegistry = (TypeRegistry)DependenciesContext.Dependencies.GetService<ITypeRegistry>();
            typeRegistry.FindAndRegisterAll(addressablesManager.ModPrefixes);


            await _localizer.LoadAvailableCulturesAsync(addressablesManager.LocalisationAddresses);
            await _localizer.LoadLocalizationFilesAsync(_settingsRepository.Get().Culture);

            InputActions = new DefaultInputActions();

            NetworkManager.Singleton.ConnectionApprovalCallback += HandleAfterApprovalCheck;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleAfterDisconnectedFromServer;

            _playerPrefabNetObj = Prefabs.Player.GetComponent<NetworkObject>();

            // Fire-and-forget
            _ = SceneManager.LoadSceneAsync(1);
        }

        // ReSharper disable once UnusedMember.Local
        private void Start()
        {
            Debug.Log("Setting up periodic save");
            _periodicSave = new DelayedAction(15f, () => SaveData(), false);
        }

        // ReSharper disable once UnusedMember.Local
        private void FixedUpdate()
        {
            if (_serverHasBeenStarted)
            {
                _periodicSave?.TryPerformAction();
            }
            else if (NetworkManager.Singleton?.IsServer ?? false)
            {
                _serverHasBeenStarted = true;
            }
        }

        private void HandleAfterApprovalCheck(NetworkManager.ConnectionApprovalRequest approvalRequest, NetworkManager.ConnectionApprovalResponse approvalResponse)
        {
            // Payload is empty for Host
            if (approvalRequest.ClientNetworkId == NetworkManager.Singleton.LocalClientId)
            {
                ServerGameDataStore.ClientIdToConnectionPayload[approvalRequest.ClientNetworkId] = GetConnectionPaylod();
                approvalResponse.Approved = true;
                return;
            }

            var payload = System.Text.Encoding.UTF8.GetString(approvalRequest.Payload);
            var connectionPayload = JsonUtility.FromJson<ConnectionPayload>(payload);

            if (string.IsNullOrEmpty(connectionPayload.Token))
            {
                Debug.LogWarning($"User '{connectionPayload.UserId}' tried to connect with an invalid Player token");
                return;
            }

            var connectionMatch = ServerGameDataStore.ClientIdToConnectionPayload.FirstOrDefault(x => x.Value.UserId == connectionPayload.UserId);
            if (!string.IsNullOrEmpty(connectionMatch.Value.UserId))
            {
                var originalClientId = connectionMatch.Key;

                if (NetworkManager.Singleton.ConnectedClients.ContainsKey(originalClientId))
                {
                    Debug.LogWarning($"User '{connectionPayload.UserId}' is already connected");

                    approvalResponse.Reason = _localizer.Translate("ui.connect.alreadyconnected");

                    return;
                }

                ServerGameDataStore.ClientIdToConnectionPayload.Remove(originalClientId);
            }

            var serverVersion = GetGameVersion();
            var clientVersion = new Version(connectionPayload.GameVersion);
            if (serverVersion.Major != clientVersion.Major || serverVersion.Minor != clientVersion.Minor)
            {
                Debug.LogWarning("Client tried to connect with an incompatible version");

                approvalResponse.Reason = _localizer.Translate("ui.connect.incompatible");

                return;
            }

            approvalResponse.Approved = true;
            ServerGameDataStore.ClientIdToConnectionPayload[approvalRequest.ClientNetworkId] = connectionPayload;

            DisconnectUserIfTokenInvalidAsync(
                approvalRequest.ClientNetworkId,
                connectionPayload.Username,
                connectionPayload.Token)
                .Forget();
        }

        private void HandleAfterDisconnectedFromServer(ulong clientId)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                ServerGameDataStore.ClientIdToConnectionPayload.Remove(clientId);
            }
            else
            {
                LocalGameDataStore.HasDisconnected = true;
                LocalGameDataStore.DisconnectReason = NetworkManager.Singleton.DisconnectReason;

                if (SceneManager.GetActiveScene().buildIndex != 1)
                {
                    SceneManager.LoadSceneAsync(1);
                }
            }
        }

        #endregion

        public async Task SetCultureAsync(string cultureCode)
        {
            await _localizer.LoadLocalizationFilesAsync(cultureCode);

            //Re-activate anything already active
            UserInterface.DebuggingOverlay.SetActive(false);
            UserInterface.DebuggingOverlay.SetActive(true);

            var gameSettings = _settingsRepository.Get();
            gameSettings.Culture = cultureCode;
            _settingsRepository.Save(gameSettings);
        }

        public async UniTask DisconnectAsync()
        {
            NetworkManager.Singleton.Shutdown();
            await SceneManager.LoadSceneAsync(1);
        }

        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit ();
#endif
        }

        public static Version GetGameVersion()
        {
            var appVersion = Application.version;
            var lastWrite = System.IO.File.GetLastWriteTime(Assembly.GetExecutingAssembly().Location);
            return new Version(appVersion + "." + lastWrite.ToString("yyyyMMdd"));
        }

        private void SaveData()
        {
            Debug.Log("SaveData() was called");
            _saveManager.ProcessQueueAsync().Forget();
        }

        public void CheckIsAdmin()
        {
            //todo: zzz v0.8 - re-enable CheckIsAdmin()
            //if (!admin)
            //{
            //    throw new Exception("You are not an admin so cannot perform that action");
            //}
        }

        private void RegisterEvents()
        {
            var eventBus = (EventBus)DependenciesContext.Dependencies.GetService<IEventBus>();

            // todo: zzz v0.6 - make these register via attribute
            eventBus.Register<ResourceValueChangedEventArgs>(LivingEntityBase.ResourceValueChangeEventId, LivingEntityBase.DefaultHandlerForResourceValueChangeEventAsync);
            eventBus.Register<ReloadEventArgs>(FighterBase.ReloadEventId, SlotStatus.DefaultHandlerForReloadEventAsync);
            eventBus.Register<ShotFiredEventArgs>(FighterBase.ShotFiredEventId, FighterBase.DefaultHandlerForShotFiredEventAsync);
            eventBus.Register<SlotChangeEventArgs>(InventoryBase.SlotChangeEventId, InventoryBase.DefaultHandlerForSlotChangeEventAsync);
        }

        private async UniTask DisconnectUserIfTokenInvalidAsync(ulong clientId, string username, string token)
        {
            if (string.IsNullOrWhiteSpace((await _userManagement.SignInWithTokenAsync(username, token)).Token))
            {
                NetworkManager.Singleton.DisconnectClient(clientId, "Invalid token");
            }
        }

        #region Methods for Mods

        private GameObject _sceneObjects;
        private ISceneBehaviour _sceneBehaviour;

        public ISceneBehaviour GetSceneBehaviour()
        {
            if (_sceneObjects == null || _sceneBehaviour == null)
            {
                _sceneObjects = _unityHelperUtilities.GetObjectAtRoot(GameObjectNames.SceneObjects);
                _sceneBehaviour = _sceneObjects.GetComponent<ISceneBehaviour>();
            }
            return _sceneBehaviour;
        }

        public IUserInterface GetUserInterface()
        {
            return UserInterface;
        }

        public GameObject GetLocalPlayerGameObject()
        {
            return LocalGameDataStore.PlayerGameObject;
        }

        private Transform GetPlayersParentTransform()
        {
            if (_playersParentTransform != null)
            {
                return _playersParentTransform;
            }

            const string playersGameObjectName = "Players";
            var parentObject = _unityHelperUtilities.GetObjectAtRoot(playersGameObjectName);
            if (parentObject == null)
            {
                parentObject = new GameObject(playersGameObjectName);
            }

            _playersParentTransform = parentObject.transform;

            return _playersParentTransform;
        }

        public void SpawnPlayerNetworkObject(Vector3 position, Quaternion rotation, ServerRpcParams serverRpcParams = default)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                Debug.LogError("Tried to create a player when not on the server");
                return;
            }

            var playerNetObj = Instantiate(_playerPrefabNetObj, position, rotation, GetPlayersParentTransform());

            var sceneService = GetSceneBehaviour().GetSceneService();
            var newPosition = sceneService.GetHeightAdjustedPosition(position, playerNetObj.GetComponent<Collider>());
            playerNetObj.transform.position = newPosition;

            var connectionPlayload = ServerGameDataStore.ClientIdToConnectionPayload[serverRpcParams.Receive.SenderClientId];
            var playerState = playerNetObj.GetComponent<PlayerFighter>();
            playerState.CharacterId = connectionPlayload.CharacterId;
            playerState.Username = connectionPlayload.Username;

            playerNetObj.SpawnAsPlayerObject(serverRpcParams.Receive.SenderClientId);
        }

        public ConnectionPayload GetConnectionPaylod()
        {
            return new ConnectionPayload
            {
                UserId = GameManager.Instance.LocalGameDataStore.SignInResult?.UserId,
                Username = GameManager.Instance.LocalGameDataStore.SignInResult?.Username,
                Token = GameManager.Instance.LocalGameDataStore.SignInResult?.Token,
                CharacterId = GameManager.Instance.LocalGameDataStore.SignInResult?.CharacterId,
                GameVersion = GameManager.GetGameVersion().ToString()
            };
        }

        #endregion
    }
}
