using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FullPotential.Api.Data;
using FullPotential.Api.GameManagement;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Ioc;
using FullPotential.Api.Localization;
using FullPotential.Api.Persistence;
using FullPotential.Api.Registry;
using FullPotential.Api.Scenes;
using FullPotential.Api.Ui;
using FullPotential.Api.Unity.Services;
using FullPotential.Api.Utilities;
using FullPotential.Api.Utilities.Extensions;
using FullPotential.Core.GameManagement.Data;
using FullPotential.Core.GameManagement.Enums;
using FullPotential.Core.GameManagement.Events;
using FullPotential.Core.Gameplay.Events;
using FullPotential.Core.Networking.Data;
using FullPotential.Core.Player;
using FullPotential.Core.Registry;
using Unity.Collections;
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
        public GameSettings GameSettings { get; private set; }
        public readonly ServerGameData ServerGameDataStore = new ServerGameData();
        public readonly LocalGameData LocalGameDataStore = new LocalGameData();

        //Events
        public event EventHandler<GameSettingsUpdatedEventArgs> GameSettingsUpdated;

        //Services
        private IUserRepository _userRepository;
        private ISettingsRepository _settingsRepository;
        private IPersistenceService _persistenceService;
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
        private async Task Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            ServerGameDataStore.ClientIdToUsername = new Dictionary<ulong, string>();
            Prefabs = GetComponent<Prefabs>();
            UserInterface = _mainCanvas.GetComponent<UserInterface>();

            ServiceManager.RegisterServices();

            _userRepository = DependenciesContext.Dependencies.GetService<IUserRepository>();
            _settingsRepository = DependenciesContext.Dependencies.GetService<ISettingsRepository>();
            _localizer = DependenciesContext.Dependencies.GetService<ILocalizer>();
            _unityHelperUtilities = DependenciesContext.Dependencies.GetService<IUnityHelperUtilities>();
            _persistenceService = DependenciesContext.Dependencies.GetService<IPersistenceService>();

            RegisterEvents();

            await UnityEngine.AddressableAssets.Addressables.InitializeAsync().Task;

            var addressablesManager = new AddressablesManager();

            var typeRegistry = (TypeRegistry)DependenciesContext.Dependencies.GetService<ITypeRegistry>();
            typeRegistry.FindAndRegisterAll(addressablesManager.ModPrefixes);

            EnsureGameSettingsLoaded();

            await _localizer.LoadAvailableCulturesAsync(addressablesManager.LocalisationAddresses);
            await _localizer.LoadLocalizationFilesAsync(GameSettings.Culture);

            InputActions = new DefaultInputActions();

            NetworkManager.Singleton.ConnectionApprovalCallback += OnApprovalCheck;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnDisconnectedFromServer;

            _playerPrefabNetObj = Prefabs.Player.GetComponent<NetworkObject>();

            SceneManager.LoadSceneAsync(1);
        }

        // ReSharper disable once UnusedMember.Local
        private void Start()
        {
            _periodicSave = new DelayedAction(15f, () => SavePlayerData(), false);
        }

        // ReSharper disable once UnusedMember.Local
        private void FixedUpdate()
        {
            if (_serverHasBeenStarted)
            {
                _periodicSave?.TryPerformAction();
            }
            else if (NetworkManager.Singleton.IsServer)
            {
                _serverHasBeenStarted = true;
            }
        }

        private void OnApprovalCheck(NetworkManager.ConnectionApprovalRequest approvalRequest, NetworkManager.ConnectionApprovalResponse approvalResponse)
        {
            if (approvalRequest.ClientNetworkId == NetworkManager.Singleton.LocalClientId)
            {
                approvalResponse.Approved = true;
                return;
            }

            var payload = System.Text.Encoding.UTF8.GetString(approvalRequest.Payload);
            var connectionPayload = JsonUtility.FromJson<ConnectionPayload>(payload);

            var playerUsername = _userRepository.GetUsernameFromToken(connectionPayload.PlayerToken);

            if (playerUsername == null)
            {
                Debug.LogWarning("Someone tried to connect with an invalid Player token");
                return;
            }

            if (ServerGameDataStore.ClientIdToUsername.ContainsValue(playerUsername))
            {
                var originalClientId = ServerGameDataStore.ClientIdToUsername.First(x => x.Value == playerUsername).Key;

                if (NetworkManager.Singleton.ConnectedClients.ContainsKey(originalClientId))
                {
                    Debug.LogWarning($"User {playerUsername} is already connected");

                    //todo: zzz v0.5 - reject reason does not seem to work yet. Approve so we can send a disconnect reason
                    approvalResponse.Approved = true;
                    SendServerToClientSetDisconnectReason(approvalRequest.ClientNetworkId, ConnectStatus.LoggedInAgain);
                    StartCoroutine(WaitToDisconnect(approvalRequest.ClientNetworkId));

                    return;
                }

                ServerGameDataStore.ClientIdToUsername.Remove(originalClientId);
            }

            var serverVersion = GetGameVersion();
            var clientVersion = new Version(connectionPayload.GameVersion);
            if (serverVersion.Major != clientVersion.Major || serverVersion.Minor != clientVersion.Minor)
            {
                Debug.LogWarning("Client tried to connect with an incompatible version");

                //todo: zzz v0.5 - reject reason does not seem to work yet. Approve so we can send a disconnect reason
                approvalResponse.Approved = true;
                SendServerToClientSetDisconnectReason(approvalRequest.ClientNetworkId, ConnectStatus.VersionMismatch);
                StartCoroutine(WaitToDisconnect(approvalRequest.ClientNetworkId));

                return;
            }

            approvalResponse.Approved = true;
        }

        private void OnDisconnectedFromServer(ulong clientId)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                ServerGameDataStore.ClientIdToUsername.Remove(clientId);
            }
            else
            {
                LocalGameDataStore.HasDisconnected = true;

                if (SceneManager.GetActiveScene().buildIndex != 1)
                {
                    SceneManager.LoadSceneAsync(1);
                }
            }
        }

        #endregion

        private void SendServerToClientSetDisconnectReason(ulong clientId, ConnectStatus status)
        {
            var writer = new FastBufferWriter(sizeof(ConnectStatus), Allocator.Temp);
            writer.WriteValueSafe(status);
            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(nameof(JoinOrHostGame.SetDisconnectReasonClientCustomMessage), clientId, writer);
        }

        private IEnumerator WaitToDisconnect(ulong clientId)
        {
            yield return new WaitForSeconds(0.5f);
            NetworkManager.Singleton.DisconnectClient(clientId);
        }

        public async Task SetCultureAsync(string cultureCode)
        {
            await _localizer.LoadLocalizationFilesAsync(cultureCode);

            //Re-activate anything already active
            UserInterface.DebuggingOverlay.SetActive(false);
            UserInterface.DebuggingOverlay.SetActive(true);

            EnsureGameSettingsLoaded();
            GameSettings.Culture = cultureCode;
        }

        private void EnsureGameSettingsLoaded()
        {
            if (!(GameSettings?.Culture).IsNullOrWhiteSpace())
            {
                return;
            }

            GameSettings = _settingsRepository.Load();

            GameSettingsUpdated?.Invoke(this, new GameSettingsUpdatedEventArgs(GameSettings));
        }

        public void Disconnect()
        {
            _periodicSave = null;

            if (NetworkManager.Singleton.IsServer)
            {
                SavePlayerData(true);
            }

            NetworkManager.Singleton.Shutdown();
            SceneManager.LoadSceneAsync(1);
        }

        public void Quit()
        {
            if (NetworkManager.Singleton.IsServer)
            {
                SavePlayerData(true);
            }

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit ();
#endif
        }

        public void SaveGameSettings()
        {
            _settingsRepository.Save(GameSettings);
            GameSettingsUpdated?.Invoke(this, new GameSettingsUpdatedEventArgs(GameSettings));
        }

        public static Version GetGameVersion()
        {
            var appVersion = Application.version;
            var lastWrite = System.IO.File.GetLastWriteTime(Assembly.GetExecutingAssembly().Location);
            return new Version(appVersion + "." + lastWrite.ToString("yyyyMMdd"));
        }

        private void SavePlayerData(bool allData = false)
        {
            _persistenceService.SaveBatchPlayerData(ServerGameDataStore.ClientIdToUsername, allData);
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
            var eventManager = (EventManager)DependenciesContext.Dependencies.GetService<IEventManager>();

            //NOTE: Before and after events because the code is updating a NetworkVariable
            eventManager.Register(LivingEntityBase.EventIdResourceValueChangeBefore, LivingEntityBase.DefaultHandlerForResourceValueBeforeChangeEvent);
            eventManager.Register(LivingEntityBase.EventIdResourceValueChangeAfter, null);

            eventManager.Register(FighterBase.EventIdReload, FighterBase.DefaultHandlerForReloadEvent);
            eventManager.Register(FighterBase.EventIdShotFired, FighterBase.DefaultHandlerForShotFiredEvent);

            eventManager.Register(InventoryBase.EventIdSlotChange, InventoryBase.DefaultHandlerForSlotChangeEvent);
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

        public string GetLocalPlayerToken()
        {
            return LocalGameDataStore.PlayerToken;
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

        public void SpawnPlayerNetworkObject(string playerToken, Vector3 position, Quaternion rotation, ServerRpcParams serverRpcParams = default)
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

            var playerState = playerNetObj.GetComponent<PlayerFighter>();
            playerState.PlayerToken = playerToken;

            playerNetObj.SpawnAsPlayerObject(serverRpcParams.Receive.SenderClientId);
        }

        #endregion

    }
}
