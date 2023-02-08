﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FullPotential.Api.GameManagement;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Gameplay.Crafting;
using FullPotential.Api.Gameplay.Data;
using FullPotential.Api.Gameplay.Drawing;
using FullPotential.Api.Gameplay.Items;
using FullPotential.Api.Ioc;
using FullPotential.Api.Localization;
using FullPotential.Api.Registry;
using FullPotential.Api.Scenes;
using FullPotential.Api.Spawning;
using FullPotential.Api.Ui;
using FullPotential.Api.Unity.Helpers;
using FullPotential.Api.Utilities;
using FullPotential.Api.Utilities.Extensions;
using FullPotential.Core.GameManagement.Data;
using FullPotential.Core.GameManagement.Enums;
using FullPotential.Core.GameManagement.Events;
using FullPotential.Core.Gameplay.Combat;
using FullPotential.Core.Gameplay.Crafting;
using FullPotential.Core.Localization;
using FullPotential.Core.Networking;
using FullPotential.Core.Networking.Data;
using FullPotential.Core.Player;
using FullPotential.Core.Registry;
using FullPotential.Core.Spawning;
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
        public CultureInfo CurrentCulture { get; private set; }

        //Input
        public DefaultInputActions InputActions { get; private set; }

        //Data Stores
        public GameSettings GameSettings { get; private set; }
        public readonly ServerGameData ServerGameDataStore = new ServerGameData();
        public readonly LocalGameData LocalGameDataStore = new LocalGameData();

        //Events
        public event EventHandler<GameSettingsUpdatedEventArgs> GameSettingsUpdated;

        //Services
        private IUserRegistry _userRegistry;
        private ILocalizer _localizer;

        //Variables
        private bool _isSaving;
        private NetworkObject _playerPrefabNetObj;
        private DelayedAction _periodicSave;
        private List<string> _asapSaveUsernames;
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

            RegisterServices();

            ServerGameDataStore.ClientIdToUsername = new Dictionary<ulong, string>();

            EnsureGameSettingsLoaded();

            await UnityEngine.AddressableAssets.Addressables.InitializeAsync().Task;
            var addressablesManager = new AddressablesManager();

            var typeRegistry = (TypeRegistry)DependenciesContext.Dependencies.GetService<ITypeRegistry>();
            typeRegistry.FindAndRegisterAll(addressablesManager.ModPrefixes);

            _userRegistry = DependenciesContext.Dependencies.GetService<IUserRegistry>();

            _localizer = DependenciesContext.Dependencies.GetService<ILocalizer>();
            await _localizer.LoadAvailableCulturesAsync(addressablesManager.LocalisationAddresses);
            await _localizer.LoadLocalizationFilesAsync(GameSettings.Culture);

            Prefabs = GetComponent<Prefabs>();
            UserInterface = _mainCanvas.GetComponent<UserInterface>();

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
            _asapSaveUsernames = new List<string>();
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

            var playerUsername = _userRegistry.GetUsernameFromToken(connectionPayload.PlayerToken);

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

                    //todo: reject reason does not seem to work yet. Approve so we can send a disconnect reason
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

        public async Task SetCultureAsync(string culture)
        {
            await _localizer.LoadLocalizationFilesAsync(culture);

            //Re-activate anything already active
            UserInterface.DebuggingOverlay.SetActive(false);
            UserInterface.DebuggingOverlay.SetActive(true);

            EnsureGameSettingsLoaded();
            GameSettings.Culture = culture;
            CurrentCulture = new CultureInfo(culture);
        }

        private static string GetGameSettingsPath()
        {
            return Application.persistentDataPath + "/LoadOptions.json";
        }

        private void EnsureGameSettingsLoaded()
        {
            if (!(GameSettings?.Culture).IsNullOrWhiteSpace())
            {
                return;
            }

            var path = GetGameSettingsPath();

            if (System.IO.File.Exists(path))
            {
                GameSettings = JsonUtility.FromJson<GameSettings>(System.IO.File.ReadAllText(path));

                //todo: zzz v0.5 - Remove GameSettings.Username backwards compat
#pragma warning disable CS0618
                if (!GameSettings.Username.IsNullOrWhiteSpace() &&
                    GameSettings.LastSigninUsername.IsNullOrWhiteSpace())
                {
                    GameSettings.LastSigninUsername = GameSettings.Username;
                    GameSettings.Username = null;
                }
#pragma warning restore CS0618

                if (GameSettings.LookSensitivity == 0)
                {
                    GameSettings.LookSensitivity = 0.2f;
                }

                if (GameSettings.LookSmoothness == 0)
                {
                    GameSettings.LookSmoothness = 3;
                }

                GameSettingsUpdated?.Invoke(this, new GameSettingsUpdatedEventArgs(GameSettings));
                return;
            }

            GameSettings = new GameSettings
            {
                Culture = Localizer.DefaultCulture
            };
        }

        public void Disconnect()
        {
            _periodicSave = null;
            _asapSaveUsernames.Clear();

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
            System.IO.File.WriteAllText(GetGameSettingsPath(), JsonUtility.ToJson(GameSettings));
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
            if (!NetworkManager.Singleton.IsServer)
            {
                Debug.LogWarning("Tried saving when not on the server");
                return;
            }

            if (_isSaving)
            {
                Debug.LogWarning("Already saving");
                return;
            }

            //Debug.Log("Checking if anything to save. allData: " + allData);

            var playerDataCollection = new List<PlayerData>();
            foreach (var kvp in NetworkManager.Singleton.ConnectedClients)
            {
                if (!ServerGameDataStore.ClientIdToUsername.ContainsKey(kvp.Key))
                {
                    Debug.LogWarning($"Could not find username for client {kvp.Key}");
                    continue;
                }

                if (allData || _asapSaveUsernames.Contains(ServerGameDataStore.ClientIdToUsername[kvp.Key]))
                {
                    playerDataCollection.Add(kvp.Value.PlayerObject.GetComponent<PlayerState>().UpdateAndReturnPlayerData());
                }
            }

            if (!playerDataCollection.Any())
            {
                return;
            }

            _isSaving = true;

            try
            {
                var tasks = playerDataCollection.Select(x => Task.Run(() => SavePlayerData(x)));
                Task.Run(async () => await Task.WhenAll(tasks))
                    .GetAwaiter()
                    .GetResult();
            }
            finally
            {
                _isSaving = false;
            }
        }

        public void QueueAsapSave(string username)
        {
            if (!_asapSaveUsernames.Contains(username))
            {
                _asapSaveUsernames.Add(username);
            }
        }

        public void SavePlayerData(PlayerData playerData)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                Debug.LogError($"Tried to save player data for '{playerData.Username}' when not on the server");
            }

            if (!playerData.InventoryLoadedSuccessfully)
            {
                Debug.LogWarning($"Not saving player data for '{playerData.Username}' because the load failed");
                return;
            }

            //Debug.Log($"Saving player data for {playerData.Username}");

            _userRegistry.Save(playerData);

            _asapSaveUsernames.Remove(playerData.Username);
        }

        private void RegisterServices()
        {
            DependenciesContext.Dependencies.Register<IUserRegistry, UserRegistry>();
            DependenciesContext.Dependencies.Register<IResultFactory, ResultFactory>();
            DependenciesContext.Dependencies.Register<IInventoryDataService, InventoryDataService>();
            DependenciesContext.Dependencies.Register<IValueCalculator, ValueCalculator>();
            DependenciesContext.Dependencies.Register<ILocalizer, Localizer>();
            DependenciesContext.Dependencies.Register<ITypeRegistry, TypeRegistry>();
            DependenciesContext.Dependencies.Register<ISpawnService, SpawnService>(true);
            DependenciesContext.Dependencies.Register<IRpcService, RpcService>();
            DependenciesContext.Dependencies.Register<IEffectService, EffectService>();
            DependenciesContext.Dependencies.Register<IModHelper, ModHelper>();
            DependenciesContext.Dependencies.Register<IDrawingService, DrawingService>();
        }

        public void CheckIsAdmin()
        {
            //Disabled while still building game
            //if (!admin)
            //{
            //    throw new Exception("You are not an admin so cannot perform that action");
            //}
        }

        #region Methods for Mods

        private GameObject _sceneObjects;
        private ISceneBehaviour _sceneBehaviour;
        public ISceneBehaviour GetSceneBehaviour()
        {
            if (_sceneObjects == null || _sceneBehaviour == null)
            {
                _sceneObjects = GameObjectHelper.GetObjectAtRoot(GameObjectNames.SceneObjects);
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
            var parentObject = GameObjectHelper.GetObjectAtRoot(playersGameObjectName);
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

            GetSceneBehaviour().GetSpawnService().AdjustPositionToBeAboveGround(position, playerNetObj.transform);

            var playerState = playerNetObj.GetComponent<PlayerState>();
            playerState.PlayerToken = playerToken;

            playerNetObj.SpawnAsPlayerObject(serverRpcParams.Receive.SenderClientId);
        }

        #endregion

    }
}
