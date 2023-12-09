using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FullPotential.Api.Data;
using FullPotential.Api.GameManagement;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Gameplay.Inventory;
using FullPotential.Api.Gameplay.Player;
using FullPotential.Api.Ioc;
using FullPotential.Api.Obsolete.Networking;
using FullPotential.Api.Obsolete.Networking.Data;
using FullPotential.Api.Persistence;
using FullPotential.Api.Registry.Resources;
using FullPotential.Api.Ui.Components;
using FullPotential.Api.Unity.Constants;
using FullPotential.Api.Unity.Services;
using FullPotential.Api.Utilities;
using FullPotential.Api.Utilities.Extensions;
using FullPotential.Core.Environment;
using FullPotential.Core.GameManagement;
using FullPotential.Core.Ui.Components;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Networking;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.Player
{
    public class PlayerFighter : FighterBase, IPlayerFighter
    {
        private const string LoadingScreenGameObjectName = "LoadingScreen";

        #region Variables

        private ClientRpcParams _clientRpcParams;

        private readonly FragmentedMessageReconstructor _loadPlayerDataReconstructor = new FragmentedMessageReconstructor();
        private readonly Dictionary<string, DateTime> _unclaimedLoot = new Dictionary<string, DateTime>();

        private string _textureUrl;
        private Vector3 _positionAfterRespawn;
        private float _myHeight;
        private MeshRenderer _bodyMeshRenderer;

        //Action-related
        private ActionQueue<bool> _aliveStateChanges;

        //Registered Services
        private IUserRepository _userRepository;
        private IUnityHelperUtilities _unityHelperUtilities;
        private IShaderUtilities _shaderUtilities;
        private IPersistenceService _persistenceService;

        //Data
        private PlayerSettings _playerSettings;
        private bool _inventoryLoadedSuccessfully;

        #endregion

        #region Inspector Variables
        // ReSharper disable UnassignedField.Global
#pragma warning disable 0649
        [SerializeField] private Behaviour[] _behavioursToDisable;
        [SerializeField] private Behaviour[] _behavioursForRespawn;
        [SerializeField] private GameObject[] _gameObjectsForPlayers;
        [SerializeField] private GameObject[] _gameObjectsForRespawn;
        [SerializeField] private Material _defaultMaterial;
        [SerializeField] private GameObject _playerCamera;
        [SerializeField] private BarSlider _healthSlider;
        public GameObject InFrontOfPlayer;
#pragma warning restore 0649
        // ReSharper restore UnassignedField.Global
        #endregion

        #region Properties

        [HideInInspector]
        public string TextureUrl
        {
            get
            {
                return _textureUrl;
            }
            private set
            {
                _textureUrl = value;
                StartCoroutine(SetTexture());
            }
        }
        [HideInInspector] public string PlayerToken { get; set; }
        [HideInInspector] public string Username { get; private set; }

        public IPlayerInventory PlayerInventory { get; private set; }

        public override Transform Transform => transform;

        public override GameObject GameObject => gameObject;

        public override Transform LookTransform => _playerCamera.transform;

        protected override IBarSlider HealthBarSlider { get; set; }

        #endregion

        #region Unity Event Handlers

        // ReSharper disable once UnusedMember.Local
        protected override void Awake()
        {
            base.Awake();

            //_stamina.OnValueChanged += OnStaminaChanged;
            //_health.OnValueChanged += OnHealthChanged;
            //_mana.OnValueChanged += OnManaChanged;
            //_energy.OnValueChanged += OnEnergyChanged;

            PlayerInventory = GetComponent<PlayerInventory>();
            _inventory = (InventoryBase)PlayerInventory;
            _bodyMeshRenderer = BodyParts.Body.GetComponent<MeshRenderer>();

            _userRepository = DependenciesContext.Dependencies.GetService<IUserRepository>();
            _unityHelperUtilities = DependenciesContext.Dependencies.GetService<IUnityHelperUtilities>();
            _shaderUtilities = DependenciesContext.Dependencies.GetService<IShaderUtilities>();
            _persistenceService = DependenciesContext.Dependencies.GetService<IPersistenceService>();

            HealthBarSlider = _healthSlider;
        }

        // ReSharper disable once UnusedMember.Local
        protected override void Start()
        {
            base.Start();

            if (IsOwner)
            {
                _unityHelperUtilities.GetObjectAtRoot(GameObjectNames.SceneCanvas).transform
                    .Find(LoadingScreenGameObjectName).gameObject
                    .SetActive(false);

                GameManager.Instance.LocalGameDataStore.PlayerGameObject = gameObject;

                foreach (var obj in _gameObjectsForPlayers)
                {
                    obj.SetActive(false);
                }
            }
            else
            {
                foreach (var comp in _behavioursToDisable)
                {
                    comp.enabled = false;
                }
            }

            gameObject.name = IsServer
                ? Username
                : "Player ID " + NetworkObjectId;

            if (IsServer)
            {
                GetAndLoadPlayerData(false, null);
                GameManager.Instance.ServerGameDataStore.ClientIdToUsername[OwnerClientId] = Username;
            }
            else if (IsOwner)
            {
                RequestPlayerDataServerRpc();
            }
            else
            {
                RequestReducedPlayerDataServerRpc();
            }

            var gameObjectCollider = gameObject.GetComponent<Collider>();
            _myHeight = gameObjectCollider.bounds.max.y - gameObjectCollider.bounds.min.y;

            if (NetworkManager.LocalClientId == OwnerClientId)
            {
                GameManager.Instance.UserInterface.Respawn.SetActive(false);

                GameManager.Instance.UserInterface.HudOverlay.Initialise(this);
                GameManager.Instance.UserInterface.Hud.SetActive(true);
            }

            QueueAliveStateChanges();
        }

        // ReSharper disable once UnusedMember.Global
        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            BodyParts.Head.rotation = _playerCamera.transform.rotation;

            BecomeVulnerable();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            _clientRpcParams.Send.TargetClientIds = new[] { OwnerClientId };
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer)
            {
                _persistenceService.SavePlayerData(GetPlayerSaveData());
            }
        }

        #endregion

        #region ServerRpc calls

        [ServerRpc]
        private void RequestPlayerDataServerRpc(ServerRpcParams serverRpcParams = default)
        {
            GetAndLoadPlayerData(false, serverRpcParams.Receive.SenderClientId);
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestReducedPlayerDataServerRpc(ServerRpcParams serverRpcParams = default)
        {
            GetAndLoadPlayerData(true, serverRpcParams.Receive.SenderClientId);
        }

        [ServerRpc]
        private void RespawnServerRpc()
        {
            SetResourceValuesForRespawn();

            AliveState = LivingEntityState.Respawning;

            var spawnPoint = GameManager.Instance.GetSceneBehaviour().GetSpawnPoint();

            PlayerSpawnStateChange(AliveState, spawnPoint.Position, spawnPoint.Rotation);

            var nearbyClients = _rpcService.ForNearbyPlayers(transform.position);
            PlayerSpawnStateChangeClientRpc(AliveState, spawnPoint.Position, spawnPoint.Rotation, nearbyClients);
        }

        [ServerRpc]
        public void ForceRespawnServerRpc()
        {
            _lastDamageSourceName = Username;
            _lastDamageItemName = null;
            HandleDeath();
        }

        [ServerRpc]
        private void UpdatePlayerSettingsServerRpc(PlayerSettings playerSettings)
        {
            _persistenceService.QueueAsapSave(Username);

            _playerSettings = playerSettings;

            UpdatePlayerSettings(_playerSettings);
        }

        #endregion

        #region ClientRpc calls

        // ReSharper disable once UnusedParameter.Local
        [ClientRpc]
        private void LoadPlayerDataClientRpc(string fragmentedMessageJson, ClientRpcParams clientRpcParams)
        {
            var fragmentedMessage = JsonUtility.FromJson<FragmentedMessage>(fragmentedMessageJson);

            _loadPlayerDataReconstructor.AddMessage(fragmentedMessage);
            if (!_loadPlayerDataReconstructor.HaveAllMessages(fragmentedMessage.GroupId))
            {
                return;
            }

            var playerData = JsonUtility.FromJson<PlayerData>(_loadPlayerDataReconstructor.Reconstruct(fragmentedMessage.GroupId));
            LoadFromPlayerData(playerData);

            StartCoroutine(SetTexture());
        }

        // ReSharper disable once UnusedParameter.Local
        [ClientRpc]
        private void SpawnLootChestClientRpc(string id, Vector3 position, ClientRpcParams clientRpcParams)
        {
            var sceneService = _gameManager.GetSceneBehaviour().GetSceneService();

            var prefab = GameManager.Instance.Prefabs.Environment.LootChest;
            var go = Instantiate(prefab, position, transform.rotation * Quaternion.Euler(0, 90, 0));

            go.transform.position = sceneService.GetHeightAdjustedPosition(position, go);

            go.transform.parent = GameManager.Instance.GetSceneBehaviour().GetTransform();
            go.name += " " + id;

            var lootScript = go.GetComponent<LootInteractable>();
            lootScript.UnclaimedLootId = id;
        }

        // ReSharper disable once UnusedParameter.Local
        [ClientRpc]
        private void PlayerSpawnStateChangeClientRpc(LivingEntityState state, Vector3 position, Quaternion rotation, ClientRpcParams clientRpcParams)
        {
            AliveState = state;

            PlayerSpawnStateChange(state, position, rotation);

            switch (state)
            {
                case LivingEntityState.Dead:
                    if (OwnerClientId == NetworkManager.LocalClientId)
                    {
                        GameManager.Instance.UserInterface.HideAllMenus();
                        _aliveStateChanges.PlayForwards(false);
                    }

                    _graphicsTransform.gameObject.SetActive(false);

                    break;

                case LivingEntityState.Respawning:
                    if (OwnerClientId == NetworkManager.LocalClientId)
                    {
                        _aliveStateChanges.PlayBackwards(true);
                    }

                    var bodyMaterialForRespawn = _bodyMeshRenderer.material;
                    _shaderUtilities.ChangeRenderMode(bodyMaterialForRespawn, ShaderRenderMode.Fade);
                    bodyMaterialForRespawn.color = new Color(1, 1, 1, 0.2f);
                    ApplyMaterial(bodyMaterialForRespawn);

                    break;

                case LivingEntityState.Alive:
                    var bodyMaterial = _bodyMeshRenderer.material;
                    _shaderUtilities.ChangeRenderMode(bodyMaterial, ShaderRenderMode.Opaque);
                    ApplyMaterial(bodyMaterial);

                    break;
            }
        }

        #endregion

        private void PlayerSpawnStateChange(LivingEntityState state, Vector3 position, Quaternion rotation)
        {
            switch (state)
            {
                case LivingEntityState.Dead:
                    RigidBody.isKinematic = true;
                    RigidBody.useGravity = false;
                    GetComponent<Collider>().enabled = false;

                    transform.position = new Vector3(0, GameManager.Instance.GetSceneBehaviour().Attributes.LowestYValue - 10, 0);

                    break;

                case LivingEntityState.Respawning:
                    var sceneService = _gameManager.GetSceneBehaviour().GetSceneService();
                    transform.position = sceneService.GetHeightAdjustedPosition(position, _myHeight);

                    transform.rotation = rotation;
                    _playerCamera.transform.localEulerAngles = Vector3.zero;

                    _positionAfterRespawn = transform.position;

                    _graphicsTransform.gameObject.SetActive(true);
                    RigidBody.isKinematic = false;

                    break;

                case LivingEntityState.Alive:
                    GetComponent<Collider>().enabled = true;
                    RigidBody.useGravity = true;

                    break;
            }
        }

        private void QueueAliveStateChanges()
        {
            _aliveStateChanges = new ActionQueue<bool>();

            _aliveStateChanges.Queue(isAlive =>
            {
                foreach (var comp in _behavioursForRespawn)
                {
                    comp.enabled = isAlive;
                }
            });

            _aliveStateChanges.Queue(isAlive =>
            {
                foreach (var obj in _gameObjectsForRespawn)
                {
                    obj.SetActive(isAlive);
                }
            });

            _aliveStateChanges.Queue(isAlive => _unityHelperUtilities.GetObjectAtRoot(GameObjectNames.SceneCamera).SetActive(!isAlive));

            _aliveStateChanges.Queue(isAlive =>
            {
                if (NetworkManager.LocalClientId == OwnerClientId)
                {
                    GameManager.Instance.UserInterface.Hud.SetActive(isAlive);
                }
            });

            _aliveStateChanges.Queue(isAlive =>
            {
                if (NetworkManager.LocalClientId == OwnerClientId)
                {
                    GameManager.Instance.UserInterface.Respawn.SetActive(!isAlive);
                }
            });
        }

        private void BecomeVulnerable()
        {
            if (AliveState is LivingEntityState.Dead or LivingEntityState.Alive)
            {
                return;
            }

            if ((int)transform.position.x == 0 && (int)transform.position.z == 0)
            {
                //Wait for transform to move to spawn position
                return;
            }

            var distanceMoved = Vector3.Distance(transform.position, _positionAfterRespawn);

            if (distanceMoved > 1)
            {
                AliveState = LivingEntityState.Alive;

                PlayerSpawnStateChange(AliveState, Vector3.zero, Quaternion.identity);

                var nearbyClients = _rpcService.ForNearbyPlayers(transform.position);
                PlayerSpawnStateChangeClientRpc(AliveState, Vector3.zero, Quaternion.identity, nearbyClients);
                _positionAfterRespawn = Vector3.zero;
            }
        }

        private void GetAndLoadPlayerData(bool reduced, ulong? sendToClientId)
        {
            var playerData = _userRepository.Load(PlayerToken, null, reduced);

            if (sendToClientId.HasValue)
            {
                //Don't send data to the server. It already has it loaded
                if (sendToClientId.Value == 0)
                {
                    return;
                }

                StartCoroutine(LoadFromPlayerDataCoroutine(playerData, sendToClientId.Value));
            }
            else
            {
                //Server loading player data from player state
                LoadFromPlayerData(playerData);
                TextureUrl = playerData.Settings?.TextureUrl ?? string.Empty;

                var msg = _localizer.Translate("ui.alert.playerjoined");
                var nearbyClients = _rpcService.ForNearbyPlayersExcept(transform.position, OwnerClientId);
                ShowHudAlertClientRpc(string.Format(msg, Username), nearbyClients);
            }
        }

        //todo: zzz v0.6 - Remove? Need this to get over the key not found exception caused by too many RPC calls with large payloads
        private IEnumerator LoadFromPlayerDataCoroutine(PlayerData playerData, ulong clientId)
        {
            var clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = new[] { clientId } }
            };

            foreach (var message in _loadPlayerDataReconstructor.GetFragmentedMessages(playerData))
            {
                LoadPlayerDataClientRpc(message, clientRpcParams);
                yield return null;
            }
        }

        private void LoadFromPlayerData(PlayerData playerData)
        {
            Username = playerData.Username;
            TextureUrl = playerData.Settings.TextureUrl;

            if (IsServer)
            {
                _entityName.Value = Username;

                var nonHealthResources = GetResources().Where(x => x.TypeId != ResourceTypeIds.Health);
                SetResourceInitialValues(nonHealthResources.ToDictionary(
                    resource => resource.TypeId.ToString(),
                    resource => playerData.Resources.FirstOrDefault(x => x.Key == resource.TypeId.ToString()).Value));
                
                var health = playerData.Resources.FirstOrDefault(kvp => kvp.Key == nameof(ResourceTypeIds.Health)).Value;
                SetResourceValue(ResourceTypeIds.HealthId, health > 0 ? health : GetResourceMax(ResourceTypeIds.HealthId));
            }

            try
            {
                ((PlayerInventory)Inventory).LoadInventory(playerData.Inventory);
                playerData.InventoryLoadedSuccessfully = true;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.ToString());
                playerData.InventoryLoadedSuccessfully = false;
            }

            _playerSettings = playerData.Settings;
            _inventoryLoadedSuccessfully = playerData.InventoryLoadedSuccessfully;

            UpdateUiHealthAndDefenceValues();
        }

        public void UpdatePlayerSettings(PlayerSettings playerSettings)
        {
            TextureUrl = playerSettings.TextureUrl;

            if (!IsServer)
            {
                UpdatePlayerSettingsServerRpc(playerSettings);
            }
        }

        private IEnumerator SetTexture()
        {
            if (Username.IsNullOrWhiteSpace())
            {
                yield break;
            }

            string filePath = null;
            if (TextureUrl != null && TextureUrl.ToLower().StartsWith("http"))
            {
                filePath = Application.persistentDataPath + "/" + Username + ".png";

                var validatePath = Application.persistentDataPath + "/" + Username + ".skinvalidate";

                var doDownload = true;

                if (System.IO.File.Exists(validatePath))
                {
                    var checkUrl = System.IO.File.ReadAllText(validatePath);
                    if (checkUrl.Equals(TextureUrl, StringComparison.OrdinalIgnoreCase))
                    {
                        doDownload = false;
                    }
                }

                if (doDownload)
                {
                    using (var webRequest = UnityWebRequest.Get(TextureUrl))
                    {
                        yield return webRequest.SendWebRequest();

                        if (webRequest.downloadHandler.data == null)
                        {
                            Debug.LogError("Failed to download texture");
                            yield break;
                        }

                        System.IO.File.WriteAllBytes(filePath, webRequest.downloadHandler.data);
                        System.IO.File.WriteAllText(validatePath, TextureUrl);
                    }
                }
            }

            Material newMat;

            if (string.IsNullOrWhiteSpace(filePath) || !System.IO.File.Exists(filePath))
            {
                ColorUtility.TryParseHtmlString("#2ADB72", out var color);
                newMat = new Material(_bodyMeshRenderer.material.shader)
                {
                    color = color
                };
            }
            else
            {
                var tex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
                tex.LoadImage(System.IO.File.ReadAllBytes(filePath));

                newMat = new Material(_bodyMeshRenderer.material.shader)
                {
                    mainTexture = tex
                };
            }

            ApplyMaterial(newMat);
        }

        protected override void HandleDeathAfter()
        {
            PlayerSpawnStateChange(AliveState, Vector3.zero, Quaternion.identity);

            var nearbyClients = _rpcService.ForNearbyPlayers(transform.position);
            PlayerSpawnStateChangeClientRpc(AliveState, Vector3.zero, Quaternion.identity, nearbyClients);
        }

        public void SpawnLootChest(Vector3 position)
        {
            ClearExpiredLoot();

            var id = Guid.NewGuid().ToString();

            _unclaimedLoot.Add(id, DateTime.UtcNow.AddHours(1));

            var clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = new[] { OwnerClientId } }
            };
            SpawnLootChestClientRpc(id, position, clientRpcParams);
        }

        public bool ClaimLoot(string id)
        {
            if (!_unclaimedLoot.ContainsKey(id))
            {
                Debug.LogError($"Could not find loot with ID {id}");
                return false;
            }

            return _unclaimedLoot.Remove(id);
        }

        private void ClearExpiredLoot()
        {
            var expiredLoot = _unclaimedLoot.Where(x => x.Value < DateTime.UtcNow).ToList();

            if (!expiredLoot.Any())
            {
                return;
            }

            foreach (var loot in expiredLoot)
            {
                _unclaimedLoot.Remove(loot.Key);
            }
        }

        public void Respawn()
        {
            RespawnServerRpc();
        }

        private void ApplyMaterial(Material material)
        {
            BodyParts.Head.GetComponent<MeshRenderer>().material = material;
            _bodyMeshRenderer.material = material;
            BodyParts.LeftArm.GetComponent<MeshRenderer>().material = material;
            BodyParts.RightArm.GetComponent<MeshRenderer>().material = material;
        }

        #region UI Updates

        public void ShowAlertForItemsAddedToInventory(string alertText)
        {
            ShowHudAlertClientRpc(alertText, _clientRpcParams);
        }

        public void AlertOfInventoryRemovals(int itemsRemovedCount)
        {
            var message = _localizer.Translate("ui.alert.itemsremoved");
            ShowHudAlertClientRpc(string.Format(message, itemsRemovedCount), _clientRpcParams);
        }

        public void AlertInventoryIsFull()
        {
            ShowHudAlertClientRpc(_localizer.Translate("ui.alert.itemsatmax"), _clientRpcParams);
        }

        #endregion

        public PlayerData GetPlayerSaveData()
        {
            var saveData = new PlayerData
            {
                Username = Username,
                Settings = _playerSettings,
                Resources = GetResources()
                    .Select(resource => new SerializableKeyValuePair<string, int>(
                        resource.TypeId.ToString(),
                        GetResourceValue(resource.TypeId.ToString())))
                    .ToArray(),
                Inventory = ((PlayerInventory)PlayerInventory).GetInventorySaveData(),
                InventoryLoadedSuccessfully = _inventoryLoadedSuccessfully
            };

            return saveData;
        }
    }
}
