using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Gameplay.Enums;
using FullPotential.Api.Gameplay.Inventory;
using FullPotential.Api.Registry;
using FullPotential.Api.Unity.Helpers;
using FullPotential.Api.Utilities;
using FullPotential.Api.Utilities.Extensions;
using FullPotential.Core.Environment;
using FullPotential.Core.GameManagement.Constants;
using FullPotential.Core.Gameplay.Combat;
using FullPotential.Core.Gameplay.Data;
using FullPotential.Core.Networking;
using FullPotential.Core.Networking.Data;
using FullPotential.Core.Registry;
using FullPotential.Core.Utilities.Extensions;
using FullPotential.Core.Utilities.Helpers;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Networking;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.PlayerBehaviours
{
    public class PlayerState : FighterBase
    {
        #region Variables

        private ClientRpcParams _clientRpcParams;

        private readonly FragmentedMessageReconstructor _loadPlayerDataReconstructor = new FragmentedMessageReconstructor();
        private readonly Dictionary<string, DateTime> _unclaimedLoot = new Dictionary<string, DateTime>();

        private string _textureUrl;
        private Vector3 _startingPosition;
        private float _myHeight;
        private MeshRenderer _bodyMeshRenderer;

        //Action-related
        private ActionQueue<bool> _aliveStateChanges;

        //Registered Services
        private UserRegistry _userRegistry;

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
        public GameObject InFrontOfPlayer;
        public Transform GraphicsTransform;

#pragma warning restore 0649
        // ReSharper enable UnassignedField.Global
        #endregion

        #region Properties

        [HideInInspector] public string TextureUrl {
            get
            {
                return _textureUrl;
            }
            set
            {
                _textureUrl = value;
                StartCoroutine(SetTexture());
            }
        }
        [HideInInspector] public string PlayerToken { get; set; }
        [HideInInspector] public string Username { get; set; }

        public IPlayerInventory Inventory { get; private set; }

        public override Transform Transform => transform;

        public override GameObject GameObject => gameObject;

        public override Transform LookTransform => _playerCamera.transform;

        public override string FighterName => Username;

        #endregion

        #region Unity Event Handlers

        // ReSharper disable once UnusedMember.Local
        protected override void Awake()
        {
            base.Awake();

            //_stamina.OnValueChanged += OnStaminaChanged;
            _health.OnValueChanged += OnHealthChanged;
            //_mana.OnValueChanged += OnManaChanged;
            //_energy.OnValueChanged += OnEnergyChanged;

            Inventory = GetComponent<PlayerInventory>();
            _inventory = Inventory;
            _bodyMeshRenderer = BodyParts.Body.GetComponent<MeshRenderer>();

            _userRegistry = _gameManager.GetService<UserRegistry>();
            _typeRegistry = _gameManager.GetService<ITypeRegistry>();
            _effectService = _gameManager.GetService<IEffectService>();
        }

        // ReSharper disable once UnusedMember.Local
        protected override void Start()
        {
            base.Start();

            if (IsOwner)
            {
                GameObjectHelper.GetObjectAtRoot(GameObjectNames.SceneCanvas).SetActive(false);
                _gameManager.LocalGameDataStore.GameObject = gameObject;

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

            gameObject.name = "Player ID " + NetworkObjectId;

            if (IsServer)
            {
                GetAndLoadPlayerData(false, null);

                if (_gameManager.GameDataStore.ClientIdToUsername.ContainsKey(OwnerClientId))
                {
                    _gameManager.GameDataStore.ClientIdToUsername[OwnerClientId] = Username;
                }
                else
                {
                    _gameManager.GameDataStore.ClientIdToUsername.Add(OwnerClientId, Username);
                }
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
                _gameManager.MainCanvasObjects.Respawn.SetActive(false);

                _gameManager.MainCanvasObjects.HudOverlay.Initialise(this);
                _gameManager.MainCanvasObjects.Hud.SetActive(true);
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

        #endregion

        #region NetworkVariable Event Handlers

        private void OnHealthChanged(int previousValue, int newValue)
        {
            UpdateHealthAndDefenceValues();
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
            _stamina.Value = GetStaminaMax();
            _health.Value = GetHealthMax();
            _mana.Value = GetManaMax();
            _energy.Value = GetEnergyMax();

            AliveState = LivingEntityState.Respawning;

            var spawnPoint = _gameManager.GetSceneBehaviour().GetSpawnPoint(gameObject);

            PlayerSpawnStateChangeBothSides(AliveState, spawnPoint.Position, spawnPoint.Rotation);

            var nearbyClients = _rpcService.ForNearbyPlayers(transform.position);
            PlayerSpawnStateChangeClientRpc(AliveState, spawnPoint.Position, spawnPoint.Rotation, null, null, nearbyClients);
        }

        [ServerRpc]
        public void ForceRespawnServerRpc()
        {
            HandleDeath(Username, null);
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

            SetName();

            StartCoroutine(SetTexture());
        }

        // ReSharper disable once UnusedParameter.Local
        [ClientRpc]
        private void SpawnLootChestClientRpc(string id, Vector3 position, ClientRpcParams clientRpcParams)
        {
            var prefab = _gameManager.Prefabs.Environment.LootChest;

            var go = Instantiate(prefab, position, transform.rotation * Quaternion.Euler(0, 90, 0));

            _gameManager.GetSceneBehaviour().GetSpawnService().AdjustPositionToBeAboveGround(position, go.transform, false);

            go.transform.parent = _gameManager.GetSceneBehaviour().GetTransform();
            go.name = id;

            var lootScript = go.GetComponent<LootInteractable>();
            lootScript.UnclaimedLootId = id;
        }

        // ReSharper disable once UnusedParameter.Local
        [ClientRpc]
        private void PlayerSpawnStateChangeClientRpc(LivingEntityState state, Vector3 position, Quaternion rotation, string killerName, string itemName, ClientRpcParams clientRpcParams)
        {
            if (!killerName.IsNullOrWhiteSpace())
            {
                var deathMessage = GetDeathMessage(IsOwner, Username, killerName, itemName);
                _gameManager.MainCanvasObjects.HudOverlay.ShowAlert(deathMessage);
            }

            PlayerSpawnStateChangeBothSides(state, position, rotation);

            switch (state)
            {
                case LivingEntityState.Dead:
                    if (OwnerClientId == NetworkManager.LocalClientId)
                    {
                        _gameManager.MainCanvasObjects.HideAllMenus();
                        _aliveStateChanges.PlayForwards(false);
                    }

                    GraphicsTransform.gameObject.SetActive(false);

                    break;

                case LivingEntityState.Respawning:
                    if (OwnerClientId == NetworkManager.LocalClientId)
                    {
                        _aliveStateChanges.PlayBackwards(true);
                    }

                    var bodyMaterialForRespawn = _bodyMeshRenderer.material;
                    ShaderHelper.ChangeRenderMode(bodyMaterialForRespawn, ShaderHelper.BlendMode.Fade);
                    bodyMaterialForRespawn.color = new Color(1, 1, 1, 0.2f);
                    ApplyMaterial(bodyMaterialForRespawn);

                    break;

                case LivingEntityState.Alive:
                    var bodyMaterial = _bodyMeshRenderer.material;
                    ShaderHelper.ChangeRenderMode(bodyMaterial, ShaderHelper.BlendMode.Opaque);
                    ApplyMaterial(bodyMaterial);

                    break;
            }
        }

        #endregion

        private void PlayerSpawnStateChangeBothSides(LivingEntityState state, Vector3 position, Quaternion rotation)
        {
            switch (state)
            {
                case LivingEntityState.Dead:
                    RigidBody.isKinematic = true;
                    RigidBody.useGravity = false;
                    GetComponent<Collider>().enabled = false;

                    transform.position = new Vector3(0, _gameManager.GetSceneBehaviour().Attributes.LowestYValue - 10, 0);

                    break;

                case LivingEntityState.Respawning:
                    _gameManager.GetSceneBehaviour().GetSpawnService().AdjustPositionToBeAboveGround(position, transform, _myHeight);

                    transform.rotation = rotation;
                    _playerCamera.transform.localEulerAngles = Vector3.zero;

                    _startingPosition = transform.position;

                    GraphicsTransform.gameObject.SetActive(true);
                    RigidBody.isKinematic = false;

                    break;

                case LivingEntityState.Alive:
                    GetComponent<Collider>().enabled = true;
                    RigidBody.useGravity = true;

                    break;
            }
        }

        public override int GetDefenseValue()
        {
            return Inventory.GetDefenseValue();
        }

        public void UpdateHealthAndDefenceValues()
        {
            if (IsOwner)
            {
                return;
            }

            var health = GetHealth();
            var maxHealth = GetHealthMax();
            var defence = Inventory.GetDefenseValue();
            var values = _healthSlider.GetHealthValues(health, maxHealth, defence);
            _healthSlider.SetValues(values);
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

            _aliveStateChanges.Queue(isAlive => GameObjectHelper.GetObjectAtRoot(GameObjectNames.SceneCamera).SetActive(!isAlive));

            _aliveStateChanges.Queue(isAlive =>
            {
                if (NetworkManager.LocalClientId == OwnerClientId)
                {
                    _gameManager.MainCanvasObjects.Hud.SetActive(isAlive);
                }
            });

            _aliveStateChanges.Queue(isAlive =>
            {
                if (NetworkManager.LocalClientId == OwnerClientId)
                {
                    _gameManager.MainCanvasObjects.Respawn.SetActive(!isAlive);
                }
            });
        }

        private void BecomeVulnerable()
        {
            if (AliveState == LivingEntityState.Dead || _startingPosition == Vector3.zero)
            {
                return;
            }

            var distanceMoved = Vector3.Distance(transform.position, _startingPosition);

            if (distanceMoved > 1)
            {
                AliveState = LivingEntityState.Alive;

                PlayerSpawnStateChangeBothSides(AliveState, Vector3.zero, Quaternion.identity);

                var nearbyClients = _rpcService.ForNearbyPlayers(transform.position);
                PlayerSpawnStateChangeClientRpc(AliveState, Vector3.zero, Quaternion.identity, null, null, nearbyClients);
                _startingPosition = Vector3.zero;
            }
        }

        private void GetAndLoadPlayerData(bool reduced, ulong? sendToClientId)
        {
            var playerData = _userRegistry.Load(PlayerToken, null, reduced);

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
                TextureUrl = playerData?.Settings?.TextureUrl ?? string.Empty;

                var msg = _localizer.Translate("ui.alert.playerjoined");
                var nearbyClients = _rpcService.ForNearbyPlayersExcept(transform.position, OwnerClientId);
                _gameManager.GetSceneBehaviour().MakeAnnouncementClientRpc(string.Format(msg, Username), nearbyClients);
            }
        }

        //NOTE: Need this to get over the key not found exception caused by too many RPC calls with large payloads
        private IEnumerator LoadFromPlayerDataCoroutine(PlayerData playerData, ulong clientId)
        {
            var clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = new[] { clientId } }
            };

            foreach (var message in FragmentedMessageReconstructor.GetFragmentedMessages(playerData))
            {
                LoadPlayerDataClientRpc(message, clientRpcParams);
                yield return null;
            }
        }

        private void LoadFromPlayerData(PlayerData playerData)
        {
            Username = playerData.Username;
            TextureUrl = playerData.Settings.TextureUrl;

            SetName();

            if (IsServer)
            {
                _health.Value = GetHealthMax();
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

            if (_userRegistry.PlayerData.ContainsKey(playerData.Username))
            {
                Debug.LogWarning($"Overwriting player data for username '{playerData.Username}'");
                _userRegistry.PlayerData[playerData.Username] = playerData;
            }
            else
            {
                _userRegistry.PlayerData.Add(playerData.Username, playerData);
            }

            UpdateHealthAndDefenceValues();
        }

        private void SetName()
        {
            var displayName = string.IsNullOrWhiteSpace(Username)
                ? "Player " + NetworkObjectId
                : Username;

            gameObject.name = displayName;

            if (IsOwner)
            {
                return;
            }

            _nameTag.text = displayName;
        }

        private IEnumerator SetTexture()
        {
            if (Username.IsNullOrWhiteSpace())
            {
                yield break;
            }

            string filePath = null;
            if (TextureUrl.ToLower().StartsWith("http"))
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

            if (filePath == null || !System.IO.File.Exists(filePath))
            {
                Debug.LogWarning("Not applying player texture because the file does not exist");

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

        public override void HandleDeath(string killerName, string itemName)
        {
            base.HandleDeath(killerName, itemName);

            PlayerSpawnStateChangeBothSides(AliveState, Vector3.zero, Quaternion.identity);

            if (killerName == Username)
            {
                killerName = _localizer.Translate("ui.alert.suicide");
            }

            var nearbyClients = _rpcService.ForNearbyPlayers(transform.position);
            PlayerSpawnStateChangeClientRpc(AliveState, Vector3.zero, Quaternion.identity, killerName, itemName, nearbyClients);
        }

        public void SpawnLootChest(Vector3 position)
        {
            ClearExpiredLoot();

            var id = Guid.NewGuid().ToMinimisedString();

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
            var expiredLoot = _unclaimedLoot.Where(x => x.Value < DateTime.UtcNow);

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
            _gameManager.GetSceneBehaviour().MakeAnnouncementClientRpc(alertText, _clientRpcParams);
        }

        public void AlertOfInventoryRemovals(int itemsRemovedCount)
        {
            var message = _localizer.Translate("ui.alert.itemsremoved");
            _gameManager.GetSceneBehaviour().MakeAnnouncementClientRpc(string.Format(message, itemsRemovedCount), _clientRpcParams);
        }

        public void AlertInventoryIsFull()
        {
            _gameManager.GetSceneBehaviour().MakeAnnouncementClientRpc(_localizer.Translate("ui.alert.itemsatmax"), _clientRpcParams);
        }

        #endregion

    }
}
