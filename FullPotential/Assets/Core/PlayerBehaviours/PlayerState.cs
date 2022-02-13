using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FullPotential.Api.GameManagement;
using FullPotential.Api.Gameplay;
using FullPotential.Api.Gameplay.Data;
using FullPotential.Api.Gameplay.Enums;
using FullPotential.Api.Registry.Spells;
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
using FullPotential.Core.UiBehaviours.Components;
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
#pragma warning restore 0649

        public GameObject InFrontOfPlayer;
        public Transform GraphicsTransform;
        // ReSharper enable UnassignedField.Global

        // ReSharper disable MemberCanBePrivate.Global
        public readonly NetworkVariable<int> Stamina = new NetworkVariable<int>(100);
        public readonly NetworkVariable<int> Health = new NetworkVariable<int>(100);
        public readonly NetworkVariable<int> Mana = new NetworkVariable<int>(100);
        // ReSharper enable MemberCanBePrivate.Global

        public PlayerHandStatus HandStatusLeft = new PlayerHandStatus();
        public PlayerHandStatus HandStatusRight = new PlayerHandStatus();

        [HideInInspector] public string PlayerToken;
        [HideInInspector] public string Username;
        [HideInInspector] public readonly NetworkVariable<FixedString512Bytes> TextureUrl = new NetworkVariable<FixedString512Bytes>();

        private Rigidbody _rb;
        private PlayerActions _playerActions;
        private ClientRpcParams _clientRpcParams;

        private FragmentedMessageReconstructor _loadPlayerDataReconstructor = new FragmentedMessageReconstructor();
        private readonly Dictionary<string, DateTime> _unclaimedLoot = new Dictionary<string, DateTime>();
        private readonly Dictionary<ulong, long> _damageTaken = new Dictionary<ulong, long>();
        private DelayedAction _consumeStamina;
        private DelayedAction _replenishStamina;
        private DelayedAction _consumeMana;
        private DelayedAction _replenishMana;
        private DelayedAction _replenishAmmo;
        private bool _isSprinting;
        private Vector3 _startingPosition;
        private float _myHeight;
        private ActionQueue<bool> _aliveStateChanges;
        private MeshRenderer _bodyMeshRenderer;

        private IRpcHelper _rpcHelper;
        private IAttackHelper _attackHelper;
        private UserRegistry _userRegistry;
        private Localizer _localizer;

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

            Inventory = GetComponent<PlayerInventory>();
            _rb = GetComponent<Rigidbody>();
            _playerActions = GetComponent<PlayerActions>();
            _bodyMeshRenderer = BodyParts.Body.GetComponent<MeshRenderer>();

            _rpcHelper = GameManager.Instance.GetService<IRpcHelper>();
            _attackHelper = GameManager.Instance.GetService<IAttackHelper>();
            _userRegistry = GameManager.Instance.GetService<UserRegistry>();
            _localizer = GameManager.Instance.GetService<Localizer>();
        }

        // ReSharper disable once UnusedMember.Local
        private void Start()
        {
            if (IsOwner)
            {
                GameObjectHelper.GetObjectAtRoot(GameObjectNames.SceneCanvas).SetActive(false);
                GameManager.Instance.LocalGameDataStore.GameObject = gameObject;

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

                if (GameManager.Instance.GameDataStore.ClientIdToUsername.ContainsKey(OwnerClientId))
                {
                    GameManager.Instance.GameDataStore.ClientIdToUsername[OwnerClientId] = Username;
                }
                else
                {
                    GameManager.Instance.GameDataStore.ClientIdToUsername.Add(OwnerClientId, Username);
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

            //todo: attribute-based stamina consumption
            _consumeStamina = new DelayedAction(.05f, () =>
            {
                var staminaCost = GetStaminaCost();
                if (_isSprinting && Stamina.Value >= staminaCost)
                {
                    Stamina.Value -= staminaCost;
                }
            });

            //todo: attribute-based stamina recharge
            _replenishStamina = new DelayedAction(.01f, () =>
            {
                if (!_isSprinting && Stamina.Value < GetStaminaMax())
                {
                    Stamina.Value += 1;
                }
            });

            //todo: attribute-based mana consumption
            _consumeMana = new DelayedAction(1f, () =>
            {
                //todo: spells need to manage their own mana consumption or this gets complicated 

                if (HandStatusLeft.SpellBeingCastGameObject != null && !SpendMana(HandStatusLeft.SpellBeingCast, HandStatusLeft.SpellBeingCast.Targeting.IsContinuous))
                {
                    HandStatusLeft.SpellBeingCastGameObject.GetComponent<ISpellBehaviour>().StopCasting();
                    var nearbyClients = _rpcHelper.ForNearbyPlayers(transform.position);
                    StopCastingClientRpc(true, nearbyClients);
                }
                if (HandStatusRight.SpellBeingCastGameObject != null && !SpendMana(HandStatusRight.SpellBeingCast, HandStatusRight.SpellBeingCast.Targeting.IsContinuous))
                {
                    HandStatusRight.SpellBeingCastGameObject.GetComponent<ISpellBehaviour>().StopCasting();
                    var nearbyClients = _rpcHelper.ForNearbyPlayers(transform.position);
                    StopCastingClientRpc(false, nearbyClients);
                }
            });

            //todo: attribute-based mana recharge
            _replenishMana = new DelayedAction(.2f, () =>
            {
                var isConsumingMana = HandStatusLeft.SpellBeingCastGameObject != null
                    || HandStatusRight.SpellBeingCastGameObject != null;

                if (!isConsumingMana && Mana.Value < GetManaMax())
                {
                    Mana.Value += 1;
                }
            });

            //todo: animation time-based ammo reloading
            _replenishAmmo = new DelayedAction(3, () =>
            {
                if (HandStatusLeft.IsReloading && HandStatusLeft.Ammo < HandStatusLeft.AmmoMax)
                {
                    HandStatusLeft.Ammo = HandStatusLeft.AmmoMax;
                    HandStatusLeft.IsReloading = false;

                    GameManager.Instance.MainCanvasObjects.HudOverlay.UpdateHandAmmo(true, HandStatusLeft);
                    ReloadCompleteClientRpc(true, _clientRpcParams);
                }
                else if (HandStatusRight.IsReloading && HandStatusRight.Ammo < HandStatusRight.AmmoMax)
                {
                    HandStatusRight.Ammo = HandStatusRight.AmmoMax;
                    HandStatusRight.IsReloading = false;

                    GameManager.Instance.MainCanvasObjects.HudOverlay.UpdateHandAmmo(false, HandStatusRight);
                    ReloadCompleteClientRpc(false, _clientRpcParams);
                }
            });

            if (NetworkManager.LocalClientId == OwnerClientId)
            {
                GameManager.Instance.MainCanvasObjects.Respawn.SetActive(false);
                GameManager.Instance.MainCanvasObjects.Hud.SetActive(true);
            }

            QueueAliveStateChanges();
        }

        // ReSharper disable once UnusedMember.Global
        public void FixedUpdate()
        {
            _head.transform.rotation = _playerCamera.transform.rotation;

            ReplenishAndConsume();
            BecomeVulnerable();
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
                GameManager.Instance.MainCanvasObjects.HudOverlay.UpdateStaminaPercentage(newValue, GetStaminaMax());
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
                GameManager.Instance.MainCanvasObjects.HudOverlay.UpdateManaPercentage(newValue, GetManaMax());
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

            AliveState = LivingEntityState.Respawning;

            var spawnPoint = GameManager.Instance.GetSceneBehaviour().GetSpawnPoint(gameObject);

            PlayerSpawnStateChangeBothSides(AliveState, spawnPoint.Position, spawnPoint.Rotation);

            var nearbyClients = _rpcHelper.ForNearbyPlayers(transform.position);
            PlayerSpawnStateChangeClientRpc(AliveState, spawnPoint.Position, spawnPoint.Rotation, null, null, nearbyClients);
        }

        [ServerRpc]
        public void ForceRespawnServerRpc()
        {
            HandleDeath(_localizer.Translate("ui.alert.suicide"), null);
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
            _playerActions.ShowDamage(position, damage);
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

            _loadPlayerDataReconstructor = null;

            SetName();

            StartCoroutine(SetTexture());
        }

        // ReSharper disable once UnusedParameter.Local
        [ClientRpc]
        private void SpawnLootChestClientRpc(string id, Vector3 position, ClientRpcParams clientRpcParams)
        {
            var prefab = GameManager.Instance.Prefabs.Environment.LootChest;

            var go = Instantiate(prefab, position, transform.rotation * Quaternion.Euler(0, 90, 0));

            GameManager.Instance.GetSceneBehaviour().GetSpawnService().AdjustPositionToBeAboveGround(position, go.transform, false);

            go.transform.parent = GameManager.Instance.GetSceneBehaviour().GetTransform();
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
                GameManager.Instance.MainCanvasObjects.HudOverlay.ShowAlert(deathMessage);
            }

            PlayerSpawnStateChangeBothSides(state, position, rotation);

            switch (state)
            {
                case LivingEntityState.Dead:
                    if (OwnerClientId == NetworkManager.LocalClientId)
                    {
                        GameManager.Instance.MainCanvasObjects.HideAllMenus();
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
                GameManager.Instance.Prefabs.Combat.ProjectileWithTrail,
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

            GameManager.Instance.MainCanvasObjects.HudOverlay.UpdateHandAmmo(isLeftHand, leftOrRight);
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

            _playerActions.StopIfCastingSpell(leftOrRight);
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

                    transform.position = new Vector3(0, GameManager.Instance.GetSceneBehaviour().Attributes.LowestYValue - 10, 0);

                    break;

                case LivingEntityState.Respawning:
                    GameManager.Instance.GetSceneBehaviour().GetSpawnService().AdjustPositionToBeAboveGround(position, transform, _myHeight);

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
                GameManager.Instance.MainCanvasObjects.HudOverlay.UpdateHealthPercentage(health, maxHealth, defence);
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
                    GameManager.Instance.MainCanvasObjects.Hud.SetActive(isAlive);
                }
            });

            _aliveStateChanges.Queue(isAlive =>
            {
                if (NetworkManager.LocalClientId == OwnerClientId)
                {
                    GameManager.Instance.MainCanvasObjects.Respawn.SetActive(!isAlive);
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

            _consumeStamina.TryPerformAction();
            _consumeMana.TryPerformAction();
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
                GameManager.Instance.GetSceneBehaviour().MakeAnnouncementClientRpc(string.Format(msg, Username), nearbyClients);
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
            GameManager.Instance.GetSceneBehaviour().MakeAnnouncementClientRpc(alertText, _clientRpcParams);
        }

        public void AlertOfInventoryRemovals(int itemsRemovedCount)
        {
            var message = _localizer.Translate("ui.alert.itemsremoved");
            GameManager.Instance.GetSceneBehaviour().MakeAnnouncementClientRpc(string.Format(message, itemsRemovedCount), _clientRpcParams);
        }

        public void AlertInventoryIsFull()
        {
            GameManager.Instance.GetSceneBehaviour().MakeAnnouncementClientRpc(_localizer.Translate("ui.alert.itemsatmax"), _clientRpcParams);
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

        public bool SpendMana(Spell activeSpell, bool slowDrain = false)
        {
            var manaCost = GetManaCost(activeSpell);

            if (slowDrain)
            {
                manaCost /= 10;
            }

            if (Mana.Value < manaCost)
            {
                return false;
            }

            Mana.Value -= manaCost;

            return true;
        }

        //todo: attribute-based all of these
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

        public int GetManaCost(Spell activeSpell)
        {
            return 20;
        }

        public int GetHealth()
        {
            return Health.Value;
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

            if (HandStatusLeft.SpellBeingCastGameObject != null)
            {
                Destroy(HandStatusLeft.SpellBeingCastGameObject);
                HandStatusLeft.SpellBeingCastGameObject = null;
            }

            if (HandStatusRight.SpellBeingCastGameObject != null)
            {
                Destroy(HandStatusRight.SpellBeingCastGameObject);
                HandStatusRight.SpellBeingCastGameObject = null;
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

    }
}
