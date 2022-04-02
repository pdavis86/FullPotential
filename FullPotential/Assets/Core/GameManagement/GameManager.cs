using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FullPotential.Api.GameManagement;
using FullPotential.Api.GameManagement.Data;
using FullPotential.Api.Gameplay;
using FullPotential.Api.Registry;
using FullPotential.Api.Scenes;
using FullPotential.Api.Spawning;
using FullPotential.Api.Ui;
using FullPotential.Api.Unity.Helpers;
using FullPotential.Api.Utilities.Extensions;
using FullPotential.Core.GameManagement.Constants;
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
        public MainCanvasObjects MainCanvasObjects { get; private set; }

        //Input
        public DefaultInputActions InputActions;

        //Data Stores
        public AppOptions AppOptions { get; private set; }
        public readonly GameData GameDataStore = new GameData();
        public readonly LocalGameData LocalGameDataStore = new LocalGameData();

        //Services
        private UserRegistry _userRegistry;
        private Localizer _localizer;

        //Variables
        private bool _isSaving;
        private NetworkObject _playerPrefabNetObj;

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

            var typeRegistry = (TypeRegistry)GetService<ITypeRegistry>();
            typeRegistry.FindAndRegisterAll();

            _userRegistry = GetService<UserRegistry>();

            _localizer = GetService<Localizer>();
            await _localizer.LoadAvailableCulturesAsync();
            await _localizer.LoadLocalizationFilesAsync(AppOptions.Culture);

            Prefabs = GetComponent<Prefabs>();
            MainCanvasObjects = _mainCanvas.GetComponent<MainCanvasObjects>();

            InputActions = new DefaultInputActions();

            NetworkManager.Singleton.ConnectionApprovalCallback += OnApprovalCheck;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnServerDisconnectedClient;

            _playerPrefabNetObj = Prefabs.Player.GetComponent<NetworkObject>();

            SceneManager.LoadSceneAsync(1);
        }

        // ReSharper disable once UnusedMember.Local
        private void FixedUpdate()
        {
            if (!_isSaving && NetworkManager.Singleton.IsServer)
            {
                StartCoroutine(PeriodicSave());
            }
        }

        private void OnApprovalCheck(byte[] connectionData, ulong clientId, NetworkManager.ConnectionApprovedDelegate callback)
        {
            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                callback(false, null, true, null, null);
                return;
            }

            var payload = System.Text.Encoding.UTF8.GetString(connectionData);
            var connectionPayload = JsonUtility.FromJson<ConnectionPayload>(payload);
            if (!_userRegistry.ValidateToken(connectionPayload.PlayerToken))
            {
                Debug.LogWarning("Someone tried to connect with an invalid Player token");
                callback(false, null, false, null, null);
                return;
            }

            var serverVersion = GetGameVersion();
            var clientVersion = new Version(connectionPayload.GameVersion);
            if (serverVersion.Major != clientVersion.Major || serverVersion.Minor != clientVersion.Minor)
            {
                Debug.LogWarning("Client tried to connect with an incompatible version");
                SendServerToClientSetDisconnectReason(clientId, ConnectStatus.VersionMismatch);
                StartCoroutine(WaitToDisconnect(clientId));
                return;
            }

            callback(false, null, true, null, null);
        }

        private void OnServerDisconnectedClient(ulong clientId)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                var playerUsername = GameDataStore.ClientIdToUsername[clientId];

                if (playerUsername.IsNullOrWhiteSpace())
                {
                    Debug.LogError($"Could not get username for client ID {clientId}");
                    return;
                }

                _userRegistry.PlayerData.Remove(playerUsername, out var playerDataToSave);
                SavePlayerData(playerDataToSave);
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
            MainCanvasObjects.DebuggingOverlay.SetActive(false);
            MainCanvasObjects.DebuggingOverlay.SetActive(true);

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
            SaveAllPlayerData();

            NetworkManager.Singleton.Shutdown();
            SceneManager.LoadSceneAsync(1);
        }

        public void Quit()
        {
            SaveAppOptions();
            SaveAllPlayerData();

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

        private IEnumerator PeriodicSave()
        {
            const int waitSeconds = 15;

            _isSaving = true;

            yield return new WaitForSeconds(waitSeconds);

            yield return new WaitUntil(() => SaveAllPlayerDataAsync().IsCompleted);

            _isSaving = false;
        }

        private void SaveAllPlayerData()
        {
            if (_isSaving || !NetworkManager.Singleton.IsServer)
            {
                return;
            }

            Task.Run(async () => await SaveAllPlayerDataAsync())
                .GetAwaiter()
                .GetResult();
        }

        private async Task SaveAllPlayerDataAsync()
        {
            var tasks = _userRegistry.PlayerData
                .Where(x => x.Value.InventoryLoadedSuccessfully && x.Value.IsDirty)
                .Select(x => Task.Run(() => SavePlayerData(x.Value)));

            await Task.WhenAll(tasks);
        }

        private void SavePlayerData(PlayerData playerData)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                Debug.LogError("Tried to save when not on the server");
            }

            if (!playerData.InventoryLoadedSuccessfully)
            {
                Debug.LogWarning("Not saving because the load failed");
                return;
            }

            _userRegistry.Save(playerData);

            playerData.IsDirty = false;
        }

        private readonly ServiceRegistry _serviceRegistry = new ServiceRegistry();
        public T GetService<T>()
        {
            return _serviceRegistry.GetService<T>();
        }

        private void RegisterServices()
        {
            _serviceRegistry.Register<UserRegistry>();
            _serviceRegistry.Register<Localizer>();
            _serviceRegistry.Register<ResultFactory>();

            _serviceRegistry.Register<ITypeRegistry, TypeRegistry>();
            _serviceRegistry.Register<IAttackHelper, AttackHelper>();
            _serviceRegistry.Register<IRpcHelper, RpcHelper>();
            _serviceRegistry.Register<ISpawnService, SpawnService>(true);
            _serviceRegistry.Register<IEffectHelper, EffectHelper>();
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
            return MainCanvasObjects;
        }

        public string GetLocalPlayerToken()
        {
            return LocalGameDataStore.PlayerToken;
        }

        public void SpawnPlayerNetworkObject(string playerToken, Vector3 position, Quaternion rotation, ServerRpcParams serverRpcParams = default)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                Debug.LogError("Tried to create a player when not on the server");
                return;
            }

            var playerNetObj = Instantiate(_playerPrefabNetObj, position, rotation);

            var playerState = playerNetObj.GetComponent<PlayerState>();
            playerState.PlayerToken = playerToken;

            playerNetObj.SpawnAsPlayerObject(serverRpcParams.Receive.SenderClientId);

            GetSceneBehaviour().GetSpawnService().AdjustPositionToBeAboveGround(position, playerNetObj.transform);
        }

        #endregion

    }
}
