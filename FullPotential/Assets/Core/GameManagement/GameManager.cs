using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FullPotential.Api.GameManagement;
using FullPotential.Api.GameManagement.Constants;
using FullPotential.Api.GameManagement.Data;
using FullPotential.Api.Gameplay.Combat;
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
using FullPotential.Core.Gameplay.Combat;
using FullPotential.Core.Gameplay.Crafting;
using FullPotential.Core.Gameplay.Data;
using FullPotential.Core.Localization;
using FullPotential.Core.Networking;
using FullPotential.Core.Networking.Data;
using FullPotential.Core.PlayerBehaviours;
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

        //Components
        public Prefabs Prefabs { get; private set; }
        public UserInterface UserInterface { get; private set; }

        //Input
        public DefaultInputActions InputActions { get; private set; }

        //Data Stores
        public AppOptions AppOptions { get; private set; }
        public readonly GameData GameDataStore = new GameData();
        public readonly LocalGameData LocalGameDataStore = new LocalGameData();

        //Services
        private UserRegistry _userRegistry;
        private ILocalizer _localizer;
        private AddressablesManager _addressablesManager;

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
        private async void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            RegisterServices();

            GameDataStore.ClientIdToUsername = new Dictionary<ulong, string>();

            EnsureAppOptionsLoaded();

            await UnityEngine.AddressableAssets.Addressables.InitializeAsync().Task;
            _addressablesManager = new AddressablesManager();

            var typeRegistry = (TypeRegistry)GetService<ITypeRegistry>();
            typeRegistry.FindAndRegisterAll(_addressablesManager.ModPrefixes);

            _userRegistry = GetService<UserRegistry>();

            _localizer = GetService<ILocalizer>();
            await _localizer.LoadAvailableCulturesAsync(_addressablesManager.LocalisationAddresses);
            await _localizer.LoadLocalizationFilesAsync(AppOptions.Culture);

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
                _periodicSave.TryPerformAction();
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
            if (!_userRegistry.ValidateToken(connectionPayload.PlayerToken))
            {
                Debug.LogWarning("Someone tried to connect with an invalid Player token");
                return;
            }

            var serverVersion = GetGameVersion();
            var clientVersion = new Version(connectionPayload.GameVersion);
            if (serverVersion.Major != clientVersion.Major || serverVersion.Minor != clientVersion.Minor)
            {
                Debug.LogWarning("Client tried to connect with an incompatible version");
                SendServerToClientSetDisconnectReason(approvalRequest.ClientNetworkId, ConnectStatus.VersionMismatch);
                StartCoroutine(WaitToDisconnect(approvalRequest.ClientNetworkId));
                return;
            }

            approvalResponse.Approved = true;
        }

#pragma warning disable UNT0006 // Incorrect message signature
        private void OnDisconnectedFromServer(ulong clientId)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                LocalGameDataStore.HasDisconnected = true;

                if (SceneManager.GetActiveScene().buildIndex != 1)
                {
                    SceneManager.LoadSceneAsync(1);
                }
            }
        }
#pragma warning restore UNT0006 // Incorrect message signature

        #endregion

        // ReSharper disable once MemberCanBePrivate.Global
        public void SendServerToClientSetDisconnectReason(ulong clientId, ConnectStatus status)
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

            EnsureAppOptionsLoaded();
            AppOptions.Culture = culture;
        }

        private static string GetAppOptionsPath()
        {
            return Application.persistentDataPath + "/LoadOptions.json";
        }

        private void EnsureAppOptionsLoaded()
        {
            if (!(AppOptions?.Culture).IsNullOrWhiteSpace())
            {
                return;
            }

            var path = GetAppOptionsPath();

            if (System.IO.File.Exists(path))
            {
                AppOptions = JsonUtility.FromJson<AppOptions>(System.IO.File.ReadAllText(path));
                return;
            }

            AppOptions = new AppOptions
            {
                Culture = Localizer.DefaultCulture
            };
        }

        public void Disconnect()
        {
            SavePlayerData(true);

            NetworkManager.Singleton.Shutdown();
            SceneManager.LoadSceneAsync(1);
        }

        public void Quit()
        {
            SaveAppOptions();

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

        // ReSharper disable once MemberCanBePrivate.Global
        public static void SaveAppOptions()
        {
            System.IO.File.WriteAllText(GetAppOptionsPath(), JsonUtility.ToJson(Instance.AppOptions));
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
                if (!GameDataStore.ClientIdToUsername.ContainsKey(kvp.Key))
                {
                    Debug.LogWarning($"Could not find username for client {kvp.Key}");
                    continue;
                }

                if (allData || _asapSaveUsernames.Contains(GameDataStore.ClientIdToUsername[kvp.Key]))
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

            Debug.Log($"Saving player data for {playerData.Username}");

            _userRegistry.Save(playerData);

            _asapSaveUsernames.Remove(playerData.Username);
        }

        private readonly ServiceRegistry _serviceRegistry = new ServiceRegistry();
        public T GetService<T>()
        {
            return _serviceRegistry.GetService<T>();
        }

        private void RegisterServices()
        {
            _serviceRegistry.Register<UserRegistry>();
            _serviceRegistry.Register<ResultFactory>();

            _serviceRegistry.Register<ILocalizer, Localizer>();
            _serviceRegistry.Register<ITypeRegistry, TypeRegistry>();
            _serviceRegistry.Register<ISpawnService, SpawnService>(true);
            _serviceRegistry.Register<IRpcService, RpcService>();
            _serviceRegistry.Register<IEffectService, EffectService>();
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

            var playerState = playerNetObj.GetComponent<PlayerState>();
            playerState.PlayerToken = playerToken;

            playerNetObj.SpawnAsPlayerObject(serverRpcParams.Receive.SenderClientId);

            GetSceneBehaviour().GetSpawnService().AdjustPositionToBeAboveGround(position, playerNetObj.transform);
        }

        #endregion

    }
}
