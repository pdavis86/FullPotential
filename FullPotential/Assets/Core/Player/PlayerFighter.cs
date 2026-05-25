using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Cysharp.Threading.Tasks;

using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Data;
using FullPotential.Api.Data.Models;
using FullPotential.Api.Gameplay;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Gameplay.Inventory;
using FullPotential.Api.Ioc;
using FullPotential.Api.Ui;
using FullPotential.Api.Unity;
using FullPotential.Api.Unity.Constants;
using FullPotential.Api.Utilities;
using FullPotential.Api.Utilities.Extensions;
using FullPotential.Core.Environment;
using FullPotential.Core.GameManagement;
using FullPotential.Core.Registry.Resources;
using FullPotential.Core.Ui.Components;

using Unity.Netcode;

using UnityEngine;
using UnityEngine.Networking;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.Player
{
    public class PlayerFighter : FighterBase, IPlayerFighter
    {
        #region Variables

        private ClientRpcParams _clientRpcParams;

        private readonly Dictionary<string, DateTime> _unclaimedLoot = new Dictionary<string, DateTime>();

        private string _textureUrl;
        private Vector3 _positionAfterRespawn;
        private float _myHeight;
        private MeshRenderer _bodyMeshRenderer;
        private bool _isReadyToBecomeVulnerable;

        //Action-related
        private ActionQueue<bool> _aliveStateChanges;

        //Registered Services
        private ISaveManager _saveManager;
        private IPlayerManagement _playerManagement;
        private IUnityHelperUtilities _unityHelperUtilities;
        private IShaderUtilities _shaderUtilities;

        //Data
        private CharacterSettings _characterSettings;

        public bool IsDirty { get; set; }

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
                SetTextureAsync().Forget();
            }
        }

        private string _username;

        [HideInInspector]
        public string Username
        {
            get => _username;
            set => _username = value;
        }

        public IPlayerInventory PlayerInventory { get; private set; }

        public override Transform Transform => transform;

        public override GameObject GameObject => gameObject;

        public override Transform LookTransform => _playerCamera.transform;

        public Transform GraphicsTransform => _graphicsTransform;

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

            _saveManager = DependenciesContext.Dependencies.GetService<ISaveManager>();
            _playerManagement = DependenciesContext.Dependencies.GetService<IPlayerManagement>();
            _unityHelperUtilities = DependenciesContext.Dependencies.GetService<IUnityHelperUtilities>();
            _shaderUtilities = DependenciesContext.Dependencies.GetService<IShaderUtilities>();

            HealthBarSlider = _healthSlider;
        }

        // ReSharper disable once UnusedMember.Local
        protected override async void Start()
        {
            base.Start();

            if (IsOwner)
            {
                _unityHelperUtilities.GetObjectAtRoot(GameObjectNames.SceneCanvas).transform
                    .Find(GameObjectNames.LoadingScreen).gameObject
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

            await GetAndLoadPlayerDataAsync(!IsOwner);

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

        protected override void OnSynchronize<T>(ref BufferSerializer<T> serializer)
        {
            serializer.SerializeValue(ref _username);
            base.OnSynchronize(ref serializer);
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
                Task.Run(async () => await _saveManager.ProcessQueueForUsernameAsync(Username))
                    .GetAwaiter()
                    .GetResult();
            }
        }

        #endregion

        #region ServerRpc calls

        [ServerRpc]
        private void RespawnServerRpc()
        {
            SetResourceValuesForRespawn();

            AliveState = LivingEntityState.Respawning;

            var spawnPoint = GameManager.Instance.GetSceneBehaviour().GetSpawnPoint();

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
        private void UpdatePlayerSettingsServerRpc(CharacterSettings characterSettings)
        {
            IsDirty = true;
            _saveManager.AddToQueue(Username, this);

            _characterSettings = characterSettings;

            UpdatePlayerSettings(_characterSettings);
        }

        #endregion

        #region ClientRpc calls

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
            if (!IsServer)
            {
                return;
            }

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

            if (!_isReadyToBecomeVulnerable)
            {
                _isReadyToBecomeVulnerable = distanceMoved < 1;
            }
            else if (distanceMoved > 1)
            {
                _isReadyToBecomeVulnerable = false;
                AliveState = LivingEntityState.Alive;

                var nearbyClients = _rpcService.ForNearbyPlayers(transform.position);
                PlayerSpawnStateChangeClientRpc(AliveState, Vector3.zero, Quaternion.identity, nearbyClients);

                _positionAfterRespawn = Vector3.zero;
            }
        }

        private async UniTask GetAndLoadPlayerDataAsync(bool reduced)
        {
            PlayerData playerData = null;
            InventoryData inventoryData = null;

            async UniTask FetchPlayerData()
            {
                playerData = await _playerManagement.GetPlayerDataAsync(Username);
            }

            async UniTask FetchInventoryData()
            {
                inventoryData = await _playerManagement.GetInventoryDataAsync(Username, reduced);
            }

            await UniTask.WhenAll(FetchPlayerData(), FetchInventoryData());

            LoadFromPlayerData(playerData);

            // todo: zzz v0.6 - why is a PlayerInventory cast necessary?
            ((PlayerInventory)Inventory).LoadInventory(inventoryData);

            // todo: zzz v0.6 - playerjoined should be an event
            if (IsServer)
            {
                var msg = _localizer.Translate("ui.alert.playerjoined", Username);
                var nearbyClients = _rpcService.ForNearbyPlayersExcept(transform.position, OwnerClientId);
                ShowHudAlertClientRpc(msg, nearbyClients);
            }
        }

        private void LoadFromPlayerData(PlayerData playerData)
        {
            TextureUrl = playerData.Settings?.TextureUrl ?? string.Empty;
            _characterSettings = playerData.Settings;

            if (IsServer)
            {
                _entityName.Value = Username;
            }

            var health = playerData.Resources.FirstOrDefault(kvp => kvp.Key == nameof(Health));
            if (health.Value == 0)
            {
                health.Value = GetResourceMax(ResourceTypeIds.HealthId);
            }

            SetResourceInitialValues(GetResources().ToDictionary(
                resource => resource.TypeId.ToString(),
                resource => playerData.Resources.FirstOrDefault(x => x.Key == resource.TypeId.ToString()).Value));
        }

        public void UpdatePlayerSettings(CharacterSettings characterSettings)
        {
            TextureUrl = characterSettings.TextureUrl;

            if (!IsServer)
            {
                UpdatePlayerSettingsServerRpc(characterSettings);
            }
        }

        private async UniTask SetTextureAsync()
        {
            if (Username.IsNullOrWhiteSpace())
            {
                return;
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
                        await webRequest.SendWebRequest();

                        if (webRequest.downloadHandler.data == null)
                        {
                            Debug.LogError("Failed to download texture");
                            return;
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

        public PlayerData GetPlayerData()
        {
            var saveData = new PlayerData
            {
                Username = Username,
                Settings = _characterSettings,
                Resources = GetResourceArrayForSave()
            };

            return saveData;
        }
    }
}
