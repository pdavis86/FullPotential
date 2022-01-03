using System;
using System.Reflection;
using FullPotential.Core.Crafting;
using FullPotential.Core.Data;
using FullPotential.Core.Localization;
using FullPotential.Core.Registry;
using FullPotential.Core.Storage;
using Unity.Netcode;
using System.Threading.Tasks;
using FullPotential.Api.Behaviours;
using FullPotential.Core.Helpers;
using UnityEngine;
using UnityEngine.SceneManagement;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable ArrangeAccessorOwnerBody

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


        //Properties
        public readonly GameManagerData DataStore = new GameManagerData();


        //Singleton
        public static GameManager Instance { get; private set; }


        private async void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            Instance.DataStore.IsDebugging = true;

            await UnityEngine.AddressableAssets.Addressables.InitializeAsync().Task;

            TypeRegistry = new TypeRegistry();
            TypeRegistry.FindAndRegisterAll();

            UserRegistry = new UserRegistry();

            var culture = GetLastUsedCulture();
            if (string.IsNullOrWhiteSpace(culture))
            {
                culture = Localizer.DefaultCulture;
                SetLastUsedCulture(culture);
            }

            Localizer = new Localizer();
            await Localizer.LoadAvailableCulturesAsync();
            await Localizer.LoadLocalizationFilesAsync(culture);

            ResultFactory = new ResultFactory(TypeRegistry, Localizer);

            Prefabs = GetComponent<Prefabs>();
            MainCanvasObjects = _mainCanvas.GetComponent<MainCanvasObjects>();

            InputActions = new DefaultInputActions();

            NetworkManager.Singleton.ConnectionApprovalCallback += OnApprovalCheck;
            //NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnServerDisconnectedClient;
            //NetworkManager.Singleton.OnServerStarted += OnServerStarted;

            //var networkTransport = NetworkManager.Singleton.GetComponent<UNetTransport>();
            //networkTransport.OnTransportEvent += OnTransportEvent;

            SceneManager.LoadSceneAsync(1);
        }

        private void OnApprovalCheck(byte[] connectionData, ulong clientId, NetworkManager.ConnectionApprovedDelegate callback)
        {
            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                callback(false, null, true, null, null);
                return;
            }

            //todo: don't let connect if too different
            GetGameVersion();

            var payload = System.Text.Encoding.UTF8.GetString(connectionData);
            var connectionPayload = JsonUtility.FromJson<ConnectionPayload>(payload);
            if (!UserRegistry.ValidateToken(connectionPayload.PlayerToken))
            {
                Debug.LogWarning("Someone tried to connect with an invalid Player token");
                callback(false, null, false, null, null);
                return;
            }

            callback(false, null, true, null, null);
        }

        //private void OnClientConnected(ulong obj)
        //{
        //    throw new NotImplementedException();
        //}

        private void OnServerDisconnectedClient(ulong clientId)
        {
            //Debug.LogWarning("Disconnected from server");

            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                Instance.DataStore.HasDisconnected = true;

                if (SceneManager.GetActiveScene().buildIndex != 1)
                {
                    SceneManager.LoadSceneAsync(1);
                }
            }
        }

        //private void OnServerStarted()
        //{
        //    throw new NotImplementedException();
        //}

        //private void OnTransportEvent(NetworkEvent eventType, ulong clientId, ArraySegment<byte> payload, float receiveTime)
        //{
        //    var foo = "";
        //}

        public async Task SetCultureAsync(string culture)
        {
            Debug.Log("Setting language to " + culture);

            await Localizer.LoadLocalizationFilesAsync(culture);

            //Re-activate anything already active
            MainCanvasObjects.DebuggingOverlay.SetActive(false);
            MainCanvasObjects.DebuggingOverlay.SetActive(true);

            SetLastUsedCulture(culture);
        }

        private static string GetAppOptionsPath()
        {
            return Application.persistentDataPath + "/LoadOptions.json";
        }

        private static void SetLastUsedCulture(string culture)
        {
            var options = new AppOptions
            {
                Culture = culture
            };

            System.IO.File.WriteAllText(GetAppOptionsPath(), JsonUtility.ToJson(options));
        }

        public static string GetLastUsedCulture()
        {
            var path = GetAppOptionsPath();

            if (!System.IO.File.Exists(path))
            {
                return null;
            }

            var options = JsonUtility.FromJson<AppOptions>(System.IO.File.ReadAllText(path));

            return options.Culture;
        }

        public static void Disconnect()
        {
            NetworkManager.Singleton.Shutdown();
            SceneManager.LoadSceneAsync(1);
        }

        public static void Quit()
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

    }
}
