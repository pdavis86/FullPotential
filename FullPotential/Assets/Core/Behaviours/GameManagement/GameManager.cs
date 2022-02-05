using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FullPotential.Core.Crafting;
using FullPotential.Core.Data;
using FullPotential.Core.Localization;
using FullPotential.Core.Registry;
using FullPotential.Core.Storage;
using Unity.Netcode;
using System.Threading.Tasks;
using FullPotential.Api.Scenes;
using FullPotential.Core.Extensions;
using FullPotential.Core.Helpers;
using Unity.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.Behaviours.GameManagement
{
    public class GameManager : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField] private GameObject _mainCanvas;
#pragma warning restore 0649

        //Core components
        public TypeRegistry TypeRegistry { get; private set; }
        public UserRegistry UserRegistry { get; private set; }
        public Localizer Localizer { get; private set; }
        public ResultFactory ResultFactory { get; private set; }


        //Behaviours
        public Prefabs Prefabs { get; private set; }
        public MainCanvasObjects MainCanvasObjects { get; private set; }

        private GameObject _sceneObjects;
        private ISceneBehaviour _sceneBehaviour;
        public ISceneBehaviour SceneBehaviour
        {
            get
            {
                if (_sceneObjects == null || _sceneBehaviour == null)
                {
                    _sceneObjects = GameObjectHelper.GetObjectAtRoot(Constants.GameObjectNames.SceneObjects);
                    _sceneBehaviour = _sceneObjects.GetComponent<ISceneBehaviour>();
                }
                return _sceneBehaviour;
            }
        }


        //Input
        public DefaultInputActions InputActions;


        //Data Stores
        public readonly GameData GameDataStore = new GameData();
        public readonly LocalGameData LocalGameDataStore = new LocalGameData();


        //Variables
        public AppOptions AppOptions;
        private bool _isSaving;


        //Singleton
        public static GameManager Instance { get; private set; }


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

            GameDataStore.ClientIdToUsername = new Dictionary<ulong, string>();

            EnsureAppOptionsLoaded();

            await UnityEngine.AddressableAssets.Addressables.InitializeAsync().Task;

            TypeRegistry = new TypeRegistry();
            TypeRegistry.FindAndRegisterAll();

            UserRegistry = new UserRegistry();

            Localizer = new Localizer();
            await Localizer.LoadAvailableCulturesAsync();
            await Localizer.LoadLocalizationFilesAsync(AppOptions.Culture);

            ResultFactory = new ResultFactory(TypeRegistry, Localizer);

            Prefabs = GetComponent<Prefabs>();
            MainCanvasObjects = _mainCanvas.GetComponent<MainCanvasObjects>();

            InputActions = new DefaultInputActions();

            NetworkManager.Singleton.ConnectionApprovalCallback += OnApprovalCheck;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnServerDisconnectedClient;

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
            if (!UserRegistry.ValidateToken(connectionPayload.PlayerToken))
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

                UserRegistry.PlayerData.Remove(playerUsername, out var playerDataToSave);
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
            await Localizer.LoadLocalizationFilesAsync(culture);

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
            if (!AppOptions.Culture.IsNullOrWhiteSpace())
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
            var tasks = UserRegistry.PlayerData
                .Where(x => x.Value.InventoryLoadedSuccessfully && x.Value.IsDirty)
                .Select(x => Task.Run(() => SavePlayerData(x.Value)));

            await Task.WhenAll(tasks);
        }

        private static void SavePlayerData(PlayerData playerData)
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

            Instance.UserRegistry.Save(playerData);

            playerData.IsDirty = false;
        }

    }
}
