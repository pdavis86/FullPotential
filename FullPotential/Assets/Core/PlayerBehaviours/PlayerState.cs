using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FullPotential.Api.GameManagement;
using FullPotential.Api.Gameplay;
using FullPotential.Api.Gameplay.Data;
using FullPotential.Api.Gameplay.Enums;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Registry.SpellsAndGadgets;
using FullPotential.Api.Unity.Helpers;
using FullPotential.Api.Utilities;
using FullPotential.Api.Utilities.Extensions;
using FullPotential.Core.Environment;
using FullPotential.Core.GameManagement;
using FullPotential.Core.GameManagement.Constants;
using FullPotential.Core.Gameplay.Combat;
using FullPotential.Core.Gameplay.Data;
using FullPotential.Core.Localization;
using FullPotential.Core.Networking;
using FullPotential.Core.Networking.Data;
using FullPotential.Core.Registry;
using FullPotential.Core.Ui.Components;
using FullPotential.Core.Utilities.Extensions;
using FullPotential.Core.Utilities.Helpers;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Networking;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.PlayerBehaviours
{
    public class PlayerState : NetworkBehaviour, IPlayerStateBehaviour
    {
        // ReSharper disable once InconsistentNaming
        private static readonly System.Random _random = new System.Random();

        // ReSharper disable UnassignedField.Global
#pragma warning disable 0649
        [SerializeField] private Behaviour[] _behavioursToDisable;
        [SerializeField] private Behaviour[] _behavioursForRespawn;
        [SerializeField] private GameObject[] _gameObjectsForPlayers;
        [SerializeField] private GameObject[] _gameObjectsForRespawn;
        public PositionTransforms Positions;
        public BodyPartTransforms BodyParts;
        [SerializeField] private TextMeshProUGUI _nameTag;
        [SerializeField] private BarSlider _healthSlider;
        [SerializeField] private Transform _head;
        [SerializeField] private Material _defaultMaterial;
        [SerializeField] private GameObject _playerCamera;
        public GameObject InFrontOfPlayer;
        public Transform GraphicsTransform;
#pragma warning restore 0649
        // ReSharper enable UnassignedField.Global

        // ReSharper disable MemberCanBePrivate.Global
        public readonly NetworkVariable<int> Stamina = new NetworkVariable<int>(100);
        public readonly NetworkVariable<int> Health = new NetworkVariable<int>(100);
        public readonly NetworkVariable<int> Mana = new NetworkVariable<int>(100);
        public readonly NetworkVariable<int> Energy = new NetworkVariable<int>(100);
        [HideInInspector] public readonly NetworkVariable<FixedString512Bytes> TextureUrl = new NetworkVariable<FixedString512Bytes>();
        // ReSharper enable MemberCanBePrivate.Global

        public PlayerHandStatus HandStatusLeft = new PlayerHandStatus();
        public PlayerHandStatus HandStatusRight = new PlayerHandStatus();
        [HideInInspector] public string PlayerToken;
        [HideInInspector] public string Username;

        private PlayerActions _playerActions;
        private ClientRpcParams _clientRpcParams;

        private readonly FragmentedMessageReconstructor _loadPlayerDataReconstructor = new FragmentedMessageReconstructor();
        private readonly Dictionary<string, DateTime> _unclaimedLoot = new Dictionary<string, DateTime>();
        private readonly Dictionary<ulong, long> _damageTaken = new Dictionary<ulong, long>();
        private readonly Dictionary<IEffect, DateTime> _activeEffects = new Dictionary<IEffect, DateTime>();

        private Rigidbody _rb;
        private bool _isSprinting;
        private Vector3 _startingPosition;
        private float _myHeight;
        private MeshRenderer _bodyMeshRenderer;

        //Action-related
        private DelayedAction _replenishAmmo;
        private DelayedAction _replenishStamina;
        private DelayedAction _replenishMana;
        private DelayedAction _replenishEnergy;
        private DelayedAction _consumeStamina;
        private DelayedAction _consumeResource;
        private DelayedAction _updateUi;
        private ActionQueue<bool> _aliveStateChanges;

        //Registered Services
        private GameManager _gameManager;
        private UserRegistry _userRegistry;
        private Localizer _localizer;
        private IRpcHelper _rpcHelper;
        private IAttackHelper _attackHelper;

        public LivingEntityState AliveState { get; private set; }

        public IPlayerInventory Inventory { get; private set; }

        public Transform Transform => transform;

        public GameObject GameObject => gameObject;

        public GameObject CameraGameObject => _playerCamera;

        #region Event handlers

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            TextureUrl.OnValueChanged += OnTextureChanged;
            Stamina.OnValueChanged += OnStaminaChanged;
            Health.OnValueChanged += OnHealthChanged;
            Mana.OnValueChanged += OnManaChanged;
            Energy.OnValueChanged += OnEnergyChanged;

            Inventory = GetComponent<PlayerInventory>();
            _rb = GetComponent<Rigidbody>();
            _playerActions = GetComponent<PlayerActions>();
            _bodyMeshRenderer = BodyParts.Body.GetComponent<MeshRenderer>();

            _gameManager = GameManager.Instance;
            _rpcHelper = _gameManager.GetService<IRpcHelper>();
            _attackHelper = _gameManager.GetService<IAttackHelper>();
            _userRegistry = _gameManager.GetService<UserRegistry>();
            _localizer = _gameManager.GetService<Localizer>();
            //_effectHelper = _gameManager.GetService<IEffectHelper>();
        }

        // ReSharper disable once UnusedMember.Local
        private void Start()
        {
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

            AliveState = LivingEntityState.Alive;

            _replenishStamina = new DelayedAction(.01f, () =>
            {
                if (!_isSprinting && Stamina.Value < GetStaminaMax())
                {
                    //todo: xp-based stamina recharge
                    Stamina.Value += 1;
                }
            });

            _replenishMana = new DelayedAction(.2f, () =>
            {
                var isConsumingMana =
                    (HandStatusLeft.SpellOrGadgetItem is Spell && HandStatusLeft.SpellOrGadgetGameObject != null)
                    || (HandStatusRight.SpellOrGadgetItem is Spell && HandStatusRight.SpellOrGadgetGameObject != null);

                if (!isConsumingMana && Mana.Value < GetManaMax())
                {
                    //todo: xp-based mana recharge
                    Mana.Value += 1;
                }
            });

            _replenishEnergy = new DelayedAction(.2f, () =>
            {
                var isConsumingEnergy =
                    (HandStatusLeft.SpellOrGadgetItem is Gadget && HandStatusLeft.SpellOrGadgetGameObject != null)
                    || (HandStatusRight.SpellOrGadgetItem is Gadget && HandStatusRight.SpellOrGadgetGameObject != null);

                if (!isConsumingEnergy && Energy.Value < GetEnergyMax())
                {
                    //todo: xp-based energy recharge
                    Energy.Value += 1;
                }
            });

            _replenishAmmo = new DelayedAction(0.5f, () =>
            {
                if (HandStatusLeft.IsReloading && HandStatusLeft.Ammo < HandStatusLeft.AmmoMax)
                {
                    StartCoroutine(ReloadCoroutine(HandStatusLeft, true));
                }
                else if (HandStatusRight.IsReloading && HandStatusRight.Ammo < HandStatusRight.AmmoMax)
                {
                    StartCoroutine(ReloadCoroutine(HandStatusRight, false));
                }
            });

            _consumeStamina = new DelayedAction(.05f, () =>
            {
                var staminaCost = GetStaminaCost();
                if (_isSprinting && Stamina.Value >= staminaCost)
                {
                    Stamina.Value -= staminaCost / 2;
                }
            });

            _consumeResource = new DelayedAction(.5f, () =>
            {
                if (HandStatusLeft.SpellOrGadgetBehaviour != null && !ConsumeResource(HandStatusLeft.SpellOrGadgetItem, HandStatusLeft.SpellOrGadgetItem.Targeting.IsContinuous))
                {
                    HandStatusLeft.SpellOrGadgetBehaviour.Stop();
                    StopCastingClientRpc(true, _rpcHelper.ForNearbyPlayers(transform.position));
                }
                if (HandStatusRight.SpellOrGadgetBehaviour != null && !ConsumeResource(HandStatusRight.SpellOrGadgetItem, HandStatusRight.SpellOrGadgetItem.Targeting.IsContinuous))
                {
                    HandStatusRight.SpellOrGadgetBehaviour.Stop();
                    StopCastingClientRpc(false, _rpcHelper.ForNearbyPlayers(transform.position));
                }
            });

            _updateUi = new DelayedAction(1, () =>
            {
                _gameManager.MainCanvasObjects.HudOverlay.UpdateActiveEffects(GetActiveEffects());
            });

            if (NetworkManager.LocalClientId == OwnerClientId)
            {
                _gameManager.MainCanvasObjects.Respawn.SetActive(false);
                _gameManager.MainCanvasObjects.Hud.SetActive(true);
            }

            QueueAliveStateChanges();
        }

        private IEnumerator ReloadCoroutine(PlayerHandStatus handStatus, bool isLeftHand)
        {
            var item = Inventory.GetItemInSlot(isLeftHand ? SlotGameObjectName.LeftHand : SlotGameObjectName.RightHand);
            yield return new WaitForSeconds(item.Attributes.GetReloadTime());

            handStatus.Ammo = handStatus.AmmoMax;
            handStatus.IsReloading = false;

            _gameManager.MainCanvasObjects.HudOverlay.UpdateHandAmmo(isLeftHand, handStatus);
            ReloadCompleteClientRpc(isLeftHand, _clientRpcParams);
        }

        // ReSharper disable once UnusedMember.Global
        public void FixedUpdate()
        {
            _head.transform.rotation = _playerCamera.transform.rotation;

            ReplenishAndConsume();
            BecomeVulnerable();

            if (IsClient)
            {
                _updateUi.TryPerformAction();
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            _clientRpcParams.Send.TargetClientIds = new[] { OwnerClientId };
        }

        private void OnTextureChanged(FixedString512Bytes previousValue, FixedString512Bytes newValue)
        {
            StartCoroutine(SetTexture());
        }

        private void OnStaminaChanged(int previousValue, int newValue)
        {
            if (NetworkManager.LocalClientId == OwnerClientId)
            {
                _gameManager.MainCanvasObjects.HudOverlay.UpdateStaminaPercentage(newValue, GetStaminaMax());
            }
        }

        private void OnHealthChanged(int previousValue, int newValue)
        {
            UpdateHealthAndDefenceValues();
        }

        private void OnManaChanged(int previousValue, int newValue)
        {
            if (NetworkManager.LocalClientId == OwnerClientId)
            {
                _gameManager.MainCanvasObjects.HudOverlay.UpdateManaPercentage(newValue, GetManaMax());
            }
        }

        private void OnEnergyChanged(int previousValue, int newValue)
        {
            if (NetworkManager.LocalClientId == OwnerClientId)
            {
                _gameManager.MainCanvasObjects.HudOverlay.UpdateEnergyPercentage(newValue, GetEnergyMax());
            }
        }

        #endregion

        #region ServerRpc calls

        [ServerRpc]
        public void RequestPlayerDataServerRpc(ServerRpcParams serverRpcParams = default)
        {
            GetAndLoadPlayerData(false, serverRpcParams.Receive.SenderClientId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestReducedPlayerDataServerRpc(ServerRpcParams serverRpcParams = default)
        {
            GetAndLoadPlayerData(true, serverRpcParams.Receive.SenderClientId);
        }

        [ServerRpc]
        public void UpdateSprintingServerRpc(bool isSprinting)
        {
            _isSprinting = isSprinting;
        }

        [ServerRpc]
        public void RespawnServerRpc()
        {
            Stamina.Value = GetStaminaMax();
            Health.Value = GetHealthMax();
            Mana.Value = GetManaMax();
            Energy.Value = GetEnergyMax();

            AliveState = LivingEntityState.Respawning;

            var spawnPoint = _gameManager.GetSceneBehaviour().GetSpawnPoint(gameObject);

            PlayerSpawnStateChangeBothSides(AliveState, spawnPoint.Position, spawnPoint.Rotation);

            var nearbyClients = _rpcHelper.ForNearbyPlayers(transform.position);
            PlayerSpawnStateChangeClientRpc(AliveState, spawnPoint.Position, spawnPoint.Rotation, null, null, nearbyClients);
        }

        [ServerRpc]
        public void ForceRespawnServerRpc()
        {
            HandleDeath(Username, null);
        }

        [ServerRpc]
        public void ReloadServerRpc(bool isLeftHand)
        {
            var leftOrRight = isLeftHand
                ? HandStatusLeft
                : HandStatusRight;

            leftOrRight.IsReloading = true;
        }

        #endregion

        #region ClientRpc calls

        // ReSharper disable once UnusedParameter.Global
        [ClientRpc]
        public void ShowDamageClientRpc(Vector3 position, string damage, ClientRpcParams clientRpcParams)
        {
            var offsetX = (float)_random.Next(-9, 10) / 100;
            var offsetY = (float)_random.Next(-9, 10) / 100;
            var offsetZ = (float)_random.Next(-9, 10) / 100;
            var adjustedPosition = position + new Vector3(offsetX, offsetY, offsetZ);
            _playerActions.ShowDamage(adjustedPosition, damage);
        }

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
                var deathMessage = _attackHelper.GetDeathMessage(IsOwner, Username, killerName, itemName);
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

        // ReSharper disable once UnusedParameter.Global
        [ClientRpc]
        public void UsedWeaponClientRpc(Vector3 startPosition, Vector3 endPosition, ClientRpcParams clientRpcParams)
        {
            var projectile = Instantiate(
                _gameManager.Prefabs.Combat.ProjectileWithTrail,
                startPosition,
                _playerCamera.transform.rotation);

            var projectileScript = projectile.GetComponent<ProjectileWithTrail>();
            projectileScript.TargetPosition = endPosition;
            projectileScript.Speed = 500;
        }

        // ReSharper disable once UnusedParameter.Local
        [ClientRpc]
        private void ReloadCompleteClientRpc(bool isLeftHand, ClientRpcParams clientRpcParams)
        {
            var leftOrRight = isLeftHand
                ? HandStatusLeft
                : HandStatusRight;

            leftOrRight.Ammo = leftOrRight.AmmoMax;
            leftOrRight.IsReloading = false;

            _gameManager.MainCanvasObjects.HudOverlay.UpdateHandAmmo(isLeftHand, leftOrRight);
        }

        // ReSharper disable once UnusedParameter.Global
        [ClientRpc]
        public void TryToAttackClientRpc(bool isLeftHand, Vector3 position, Vector3 forward, ClientRpcParams clientRpcParams)
        {
            _playerActions.TryToAttack(isLeftHand, position, forward, this);
        }

        // ReSharper disable once UnusedParameter.Global
        [ClientRpc]
        public void StopCastingClientRpc(bool isLeftHand, ClientRpcParams clientRpcParams)
        {
            var leftOrRight = isLeftHand
                ? HandStatusLeft
                : HandStatusRight;

            _playerActions.StopSpellOrGadget(leftOrRight);
        }

        #endregion

        private void PlayerSpawnStateChangeBothSides(LivingEntityState state, Vector3 position, Quaternion rotation)
        {
            switch (state)
            {
                case LivingEntityState.Dead:
                    _rb.isKinematic = true;
                    _rb.useGravity = false;
                    GetComponent<Collider>().enabled = false;

                    transform.position = new Vector3(0, _gameManager.GetSceneBehaviour().Attributes.LowestYValue - 10, 0);

                    break;

                case LivingEntityState.Respawning:
                    _gameManager.GetSceneBehaviour().GetSpawnService().AdjustPositionToBeAboveGround(position, transform, _myHeight);

                    transform.rotation = rotation;
                    _playerCamera.transform.localEulerAngles = Vector3.zero;

                    _startingPosition = transform.position;

                    GraphicsTransform.gameObject.SetActive(true);
                    _rb.isKinematic = false;

                    break;

                case LivingEntityState.Alive:
                    GetComponent<Collider>().enabled = true;
                    _rb.useGravity = true;

                    break;
            }
        }

        public int GetDefenseValue()
        {
            return Inventory.GetDefenseValue();
        }

        public void UpdateHealthAndDefenceValues()
        {
            var health = GetHealth();
            var maxHealth = GetHealthMax();
            var defence = Inventory.GetDefenseValue();

            if (!IsOwner)
            {
                var values = _healthSlider.GetHealthValues(health, maxHealth, defence);
                _healthSlider.SetValues(values);
            }

            if (NetworkManager.LocalClientId == OwnerClientId)
            {
                _gameManager.MainCanvasObjects.HudOverlay.UpdateHealthPercentage(health, maxHealth, defence);
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

        private void ReplenishAndConsume()
        {
            _replenishAmmo.TryPerformAction();

            if (!IsServer)
            {
                return;
            }

            _replenishStamina.TryPerformAction();
            _replenishMana.TryPerformAction();
            _replenishEnergy.TryPerformAction();

            _consumeStamina.TryPerformAction();
            _consumeResource.TryPerformAction();
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

                var nearbyClients = _rpcHelper.ForNearbyPlayers(transform.position);
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
                TextureUrl.Value = playerData?.Settings?.TextureUrl ?? string.Empty;

                var msg = _localizer.Translate("ui.alert.playerjoined");
                var nearbyClients = _rpcHelper.ForNearbyPlayersExcept(transform.position, OwnerClientId);
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

        private void LoadFromPlayerData(PlayerData playerData)
        {
            Username = playerData.Username;
            SetName();

            if (IsServer)
            {
                Health.Value = GetHealthMax();
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

            var textureUrl = TextureUrl.Value.ToString();

            string filePath = null;
            if (textureUrl.ToLower().StartsWith("http"))
            {
                filePath = Application.persistentDataPath + "/" + Username + ".png";

                var validatePath = Application.persistentDataPath + "/" + Username + ".skinvalidate";

                var doDownload = true;

                if (System.IO.File.Exists(validatePath))
                {
                    var checkUrl = System.IO.File.ReadAllText(validatePath);
                    if (checkUrl.Equals(textureUrl, StringComparison.OrdinalIgnoreCase))
                    {
                        doDownload = false;
                    }
                }

                if (doDownload)
                {
                    using (var webRequest = UnityWebRequest.Get(textureUrl))
                    {
                        yield return webRequest.SendWebRequest();

                        if (webRequest.downloadHandler.data == null)
                        {
                            Debug.LogError("Failed to download texture");
                            yield break;
                        }

                        System.IO.File.WriteAllBytes(filePath, webRequest.downloadHandler.data);
                        System.IO.File.WriteAllText(validatePath, textureUrl);
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

        private (NetworkVariable<int> Variable, int? Cost)? GetResourceVariableAndCost(SpellOrGadgetItemBase spellOrGadget)
        {
            switch (spellOrGadget.ResourceConsumptionType)
            {
                case ResourceConsumptionType.Mana:
                    return (Mana, GetManaCost((Spell)spellOrGadget));

                case ResourceConsumptionType.Energy:
                    return (Energy, GetEnergyCost((Gadget)spellOrGadget));

                default:
                    Debug.LogError("Not yet implemented GetResourceVariable() for resource type " + spellOrGadget.ResourceConsumptionType);
                    return null;
            }
        }

        public bool ConsumeResource(SpellOrGadgetItemBase spellOrGadget, bool slowDrain = false, bool isTest = false)
        {
            var tuple = GetResourceVariableAndCost(spellOrGadget);

            if (!tuple.HasValue || !tuple.Value.Cost.HasValue)
            {
                Debug.LogError("Failed to get GetResourceVariableAndCost");
                return false;
            }

            var resourceCost = tuple.Value.Cost.Value;
            var resourceVariable = tuple.Value.Variable;

            if (slowDrain)
            {
                resourceCost = (int)Math.Ceiling(resourceCost / 10f) + 1;
            }

            if (resourceVariable.Value < resourceCost)
            {
                return false;
            }

            if (!isTest)
            {
                resourceVariable.Value -= resourceCost;
            }

            return true;
        }

        public int GetHealth()
        {
            return Health.Value;
        }

        //todo: xp-based max, cost, speed values
        public int GetStaminaMax()
        {
            return 100;
        }

        public int GetStaminaCost()
        {
            return 10;
        }

        public float GetSprintSpeed()
        {
            return 2.5f;
        }

        public int GetHealthMax()
        {
            return 100;
        }

        public int GetManaMax()
        {
            return 100;
        }

        public int GetEnergyMax()
        {
            return 100;
        }

        //todo: attribute-based costs
        public int GetManaCost(Spell spell)
        {
            return 20;
        }

        public int GetEnergyCost(Gadget gadget)
        {
            return 20;
        }

        public void TakeDamage(int amount, ulong? clientId, string attackerName, string itemName)
        {
            if (clientId != null && clientId != OwnerClientId)
            {
                if (_damageTaken.ContainsKey(clientId.Value))
                {
                    _damageTaken[clientId.Value] += amount;
                }
                else
                {
                    _damageTaken.Add(clientId.Value, amount);
                }
            }

            Health.Value -= amount;

            if (Health.Value <= 0)
            {
                HandleDeath(attackerName, itemName);
            }
        }

        public void HandleDeath(string killerName, string itemName)
        {
            if (killerName == Username)
            {
                killerName = _localizer.Translate("ui.alert.suicide");
            }

            foreach (var item in _damageTaken)
            {
                if (!NetworkManager.Singleton.ConnectedClients.ContainsKey(item.Key))
                {
                    continue;
                }

                var playerState = NetworkManager.Singleton.ConnectedClients[item.Key].PlayerObject.gameObject.GetComponent<PlayerState>();
                playerState.SpawnLootChest(transform.position);
            }

            _damageTaken.Clear();

            AliveState = LivingEntityState.Dead;

            PlayerSpawnStateChangeBothSides(AliveState, Vector3.zero, Quaternion.identity);

            var nearbyClients = _rpcHelper.ForNearbyPlayers(transform.position);
            PlayerSpawnStateChangeClientRpc(AliveState, Vector3.zero, Quaternion.identity, killerName, itemName, nearbyClients);

            if (HandStatusLeft.SpellOrGadgetGameObject != null)
            {
                Destroy(HandStatusLeft.SpellOrGadgetGameObject);
                HandStatusLeft.SpellOrGadgetGameObject = null;
            }

            if (HandStatusRight.SpellOrGadgetGameObject != null)
            {
                Destroy(HandStatusRight.SpellOrGadgetGameObject);
                HandStatusRight.SpellOrGadgetGameObject = null;
            }
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

        private Dictionary<IEffect, float> GetActiveEffects()
        {
            var expiredEffects = _activeEffects
                .Where(x => x.Value < DateTime.Now)
                .ToList();

            foreach (var kvp in expiredEffects)
            {
                _activeEffects.Remove(kvp.Key);
            }

            return _activeEffects.ToDictionary(
                x => x.Key, 
                x => (float)(DateTime.Now - x.Value).TotalSeconds);
        }

        public NetworkVariable<int> GetStatVariable(AffectableStats stat)
        {
            switch (stat)
            {
                case AffectableStats.Energy: return Energy;
                case AffectableStats.Health: return Health;
                case AffectableStats.Mana: return Mana;
                case AffectableStats.Stamina: return Stamina;
                default:
                    throw new NotImplementedException();
            }
        }

        public int GetStatVariableMax(AffectableStats stat)
        {
            switch (stat)
            {
                case AffectableStats.Energy: return GetEnergyMax();
                case AffectableStats.Health: return GetHealthMax();
                case AffectableStats.Mana: return GetManaMax();
                case AffectableStats.Stamina: return GetStaminaMax();
                default:
                    throw new NotImplementedException();
            }
        }

        #region Nested Classes

        // ReSharper disable UnassignedField.Global

        [Serializable]
        public struct PositionTransforms
        {
            public Transform LeftHand;
            public Transform RightHand;
            public Transform LeftHandInFront;
            public Transform RightHandInFront;
        }

        [Serializable]
        public struct BodyPartTransforms
        {
            public Transform Head;
            public Transform Body;
            public Transform LeftArm;
            public Transform RightArm;
        }

        // ReSharper enable UnassignedField.Global

        #endregion

        //todo: move these
        public void AddAttributeModifier(IAttributeEffect attributeEffect, Attributes attributes)
        {
            //todo:
            throw new NotImplementedException();
        }

        public void ApplyPeriodicActionToStat(IStatEffect statEffect, Attributes attributes)
        {
            //todo:
            throw new NotImplementedException();
        }

        public void AlterValue(IStatEffect statEffect, Attributes attributes)
        {
            var statVariable = GetStatVariable(statEffect.StatToAffect);
            var statMax = GetStatVariableMax(statEffect.StatToAffect);

            //todo: attribute-based values
            var change = 10;
            var duration = 2f;

            if (_activeEffects.ContainsKey(statEffect))
            {
                _activeEffects.Remove(statEffect);
            }
            _activeEffects.Add(statEffect, DateTime.Now.AddSeconds(duration));

            if (statVariable.Value >= statMax)
            {
                return;
            }

            if (statEffect.Affect == Affect.SingleIncrease)
            {
                if (statVariable.Value < statMax - change)
                {
                    statVariable.Value += change;
                }
                else
                {
                    statVariable.Value = statMax;
                }
                return;
            }

            if (statVariable.Value - change >= 0)
            {
                statVariable.Value -= change;
            }
            else
            {
                statVariable.Value = 0;

                //todo: other min values
                if (statVariable == Health)
                {
                    HandleDeath(Username, null); //todo: replace null
                }
            }


            
        }

        public void ApplyTemporaryMaxActionToStat(IStatEffect statEffect, Attributes attributes)
        {
            //todo:
            throw new NotImplementedException();
        }

        public Rigidbody GetRigidBody()
        {
            //todo:
            throw new NotImplementedException();
        }
    }
}
