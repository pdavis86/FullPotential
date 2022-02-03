using FullPotential.Core.Behaviours.GameManagement;
using FullPotential.Core.Data;
using FullPotential.Core.Networking;
using FullPotential.Core.Registry.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FullPotential.Api.Combat;
using FullPotential.Api.Enums;
using FullPotential.Core.Behaviours.Environment;
using FullPotential.Core.Behaviours.Ui;
using FullPotential.Core.Behaviours.UI.Components;
using FullPotential.Core.Combat;
using FullPotential.Core.Extensions;
using FullPotential.Core.Helpers;
using FullPotential.Core.Utilities;
using FullPotential.Standard.Spells.Behaviours;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Networking;

// ReSharper disable UnusedMember.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Compiler
// ReSharper disable ConvertToUsingDeclaration
// ReSharper disable UnassignedField.Global

namespace FullPotential.Core.Behaviours.PlayerBehaviours
{
    public class PlayerState : NetworkBehaviour, IDamageable
    {
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
#pragma warning restore 0649

        public GameObject PlayerCamera;
        public GameObject InFrontOfPlayer;
        public Transform GraphicsTransform;

        public readonly NetworkVariable<int> Stamina = new NetworkVariable<int>(100);
        public readonly NetworkVariable<int> Health = new NetworkVariable<int>(100);
        public readonly NetworkVariable<int> Mana = new NetworkVariable<int>(100);

        public LivingEntityState AliveState { get; set; }

        [HideInInspector] public PlayerInventory Inventory;
        [HideInInspector] public string PlayerToken;
        [HideInInspector] public string Username;
        [HideInInspector] public readonly NetworkVariable<FixedString512Bytes> TextureUrl = new NetworkVariable<FixedString512Bytes>();

        private Rigidbody _rb;
        private PlayerActions _playerActions;
        private ClientRpcParams _clientRpcParams;
        private GameObject _spellBeingCastLeft;
        private GameObject _spellBeingCastRight;
        private Hud _hud;

        private FragmentedMessageReconstructor _loadPlayerDataReconstructor = new FragmentedMessageReconstructor();
        private readonly Dictionary<string, DateTime> _unclaimedLoot = new Dictionary<string, DateTime>();
        private readonly Dictionary<ulong, long> _damageTaken = new Dictionary<ulong, long>();
        private DelayedAction _consumeStamina;
        private DelayedAction _replenishStamina;
        private DelayedAction _replenishMana;
        private bool _isSprinting;
        private Vector3 _startingPosition;
        private float _myHeight;
        private ActionQueue<bool> _aliveStateChanges;
        private MeshRenderer _bodyMeshRenderer;

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
        }

        // ReSharper disable once UnusedMember.Local
        private void Start()
        {
            if (IsOwner)
            {
                GameObjectHelper.GetObjectAtRoot(Constants.GameObjectNames.SceneCanvas).SetActive(false);
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
                //Debug.LogError("I am the Server. Loading player data with client ID " + OwnerClientId);
                GetAndLoadPlayerData(false, null);

                //Debug.Log($"Adding client ID {OwnerClientId} with username '{Username}'");
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
                //Debug.LogError("Requesting my player data with client ID " + OwnerClientId);
                RequestPlayerDataServerRpc();
            }
            else
            {
                //Debug.LogError("Requesting other player data for client ID " + OwnerClientId);
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

            //todo: attribute-based mana recharge
            _replenishMana = new DelayedAction(.2f, () =>
            {
                var isConsumingMana = _spellBeingCastLeft != null
                    || _spellBeingCastRight != null;

                if (!isConsumingMana && Mana.Value < GetManaMax())
                {
                    Mana.Value += 1;
                }
            });

            if (NetworkManager.LocalClientId == OwnerClientId)
            {
                GameManager.Instance.MainCanvasObjects.Respawn.SetActive(false);
                GameManager.Instance.MainCanvasObjects.Hud.SetActive(true);
            }

            QueueAliveStateChanges();
        }

        public void FixedUpdate()
        {
            _head.transform.rotation = PlayerCamera.transform.rotation;

            HandleSprinting();
            Replenish();
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
                GetHud().UpdateStaminaPercentage(newValue, GetStaminaMax());
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
                GetHud().UpdateManaPercentage(newValue, GetManaMax());
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
            Health.Value = GetHealthMax();
            var spawnPoint = GameManager.Instance.SceneBehaviour.GetSpawnPoint(gameObject);
            RespawnClientRpc(_clientRpcParams);

            AliveState = LivingEntityState.Respawning;
            PlayerSpawnStateChangeClientRpc(spawnPoint.Position, LivingEntityState.Respawning, null, null, RpcHelper.ForNearbyPlayers());
        }

        [ServerRpc]
        public void ForceRespawnServerRpc()
        {
            HandleDeath(GameManager.Instance.Localizer.Translate("ui.alert.suicide"), null);
        }

        #endregion

        #region ClientRpc calls

        // ReSharper disable once UnusedParameter.Global
        [ClientRpc]
        public void ShowDamageClientRpc(Vector3 position, string damage, ClientRpcParams clientRpcParams)
        {
            _playerActions.ShowDamage(position, damage);
        }

        // ReSharper disable once UnusedParameter.Global
        [ClientRpc]
        public void LoadPlayerDataClientRpc(string fragmentedMessageJson, ClientRpcParams clientRpcParams)
        {
            var fragmentedMessage = JsonUtility.FromJson<FragmentedMessage>(fragmentedMessageJson);

            //Debug.LogError($"Received part message with SequenceId {fragmentedMessage.SequenceId}");

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

        // ReSharper disable once UnusedParameter.Global
        [ClientRpc]
        public void SpawnLootChestClientRpc(string id, Vector3 position, ClientRpcParams clientRpcParams)
        {
            var prefab = GameManager.Instance.Prefabs.Environment.LootChest;

            var go = Instantiate(prefab, position, transform.rotation * Quaternion.Euler(0, 90, 0));

            GameManager.Instance.SceneBehaviour.GetSpawnService().AdjustPositionToBeAboveGround(position, go, false);

            go.transform.parent = GameManager.Instance.SceneBehaviour.GetTransform();
            go.name = id;

            var lootScript = go.GetComponent<LootInteractable>();
            lootScript.UnclaimedLootId = id;
        }

        // ReSharper disable once UnusedParameter.Global
        [ClientRpc]
        public void YouDiedClientRpc(ClientRpcParams clientRpcParams)
        {
            GameManager.Instance.MainCanvasObjects.HideAllMenus();
            _aliveStateChanges.PlayForwards(false);
        }

        // ReSharper disable once UnusedParameter.Global
        [ClientRpc]
        public void RespawnClientRpc(ClientRpcParams clientRpcParams)
        {
            _aliveStateChanges.PlayBackwards(true);
        }

        // ReSharper disable once UnusedParameter.Global
        [ClientRpc]
        public void PlayerSpawnStateChangeClientRpc(Vector3 position, LivingEntityState state, string killerName, string itemName, ClientRpcParams clientRpcParams)
        {
            if (!killerName.IsNullOrWhiteSpace())
            {
                var deathMessage = AttackHelper.GetDeathMessage(IsOwner, Username, killerName, itemName);
                GameManager.Instance.MainCanvasObjects.Hud.GetComponent<Hud>().ShowAlert(deathMessage);
            }

            switch (state)
            {
                case LivingEntityState.Dead:
                    GraphicsTransform.gameObject.SetActive(false);
                    _rb.isKinematic = true;
                    _rb.useGravity = false;
                    GetComponent<Collider>().enabled = false;
                    break;

                case LivingEntityState.Respawning:
                    GraphicsTransform.gameObject.SetActive(true);
                    _rb.isKinematic = false;

                    //Don't halve it as object can still end up in the floor
                    position.y += _myHeight / 1.5f;

                    _startingPosition = position;

                    var bodyMaterialForRespawn = _bodyMeshRenderer.material;
                    ShaderHelper.ChangeRenderMode(bodyMaterialForRespawn, ShaderHelper.BlendMode.Fade);
                    bodyMaterialForRespawn.color = new Color(1, 1, 1, 0.2f);
                    ApplyMaterial(bodyMaterialForRespawn);
                    break;

                case LivingEntityState.Alive:
                    GetComponent<Collider>().enabled = true;
                    _rb.useGravity = true;

                    var bodyMaterial = _bodyMeshRenderer.material;
                    ShaderHelper.ChangeRenderMode(bodyMaterial, ShaderHelper.BlendMode.Opaque);
                    ApplyMaterial(bodyMaterial);

                    break;
            }
        }

        #endregion

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
                GetHud().UpdateHealthPercentage(health, maxHealth, defence);
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

            _aliveStateChanges.Queue(isAlive => GameObjectHelper.GetObjectAtRoot(Constants.GameObjectNames.SceneCamera).SetActive(!isAlive));

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

        private Hud GetHud()
        {
            if (_hud == null)
            {
                _hud = GameManager.Instance.MainCanvasObjects.Hud.GetComponent<Hud>();
            }

            return _hud;
        }

        private void HandleSprinting()
        {
            if (!IsServer)
            {
                return;
            }

            _consumeStamina.TryPerformAction();
        }

        private void Replenish()
        {
            if (!IsServer)
            {
                return;
            }

            _replenishStamina.TryPerformAction();
            _replenishMana.TryPerformAction();
        }

        private void BecomeVulnerable()
        {
            if (_startingPosition == Vector3.zero)
            {
                return;
            }

            var distanceMoved = Vector3.Distance(transform.position, _startingPosition);

            //Debug.Log("Distance moved since death: " + distanceMoved + ". _startingPosition: " + _startingPosition + ". Current pos: " + transform.position);

            if (distanceMoved > 1)
            {
                AliveState = LivingEntityState.Alive;
                PlayerSpawnStateChangeClientRpc(Vector3.zero, LivingEntityState.Alive, null, null, RpcHelper.ForNearbyPlayers());
                _startingPosition = Vector3.zero;
            }
        }

        private void GetAndLoadPlayerData(bool reduced, ulong? sendToClientId)
        {
            //Debug.Log($"Loading player data for {OwnerClientId}, reduced: {reduced}");

            var playerData = GameManager.Instance.UserRegistry.Load(PlayerToken, null, reduced);

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
            }
        }

        //NOTE: Need this to get over the key not found exception caused by too many RPC calls with large payloads
        private IEnumerator LoadFromPlayerDataCoroutine(PlayerData playerData, ulong clientId)
        {
            var clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = new[] { clientId } }
            };

            //Debug.LogError($"Sending LoadFromPlayerData messages to client {clientId}");

            foreach (var message in FragmentedMessageReconstructor.GetFragmentedMessages(playerData))
            {
                LoadPlayerDataClientRpc(message, clientRpcParams);
                yield return null;
            }
        }

        public static PlayerState GetWithClientId(ulong clientId)
        {
            var playerObjects = GameObject.FindGameObjectsWithTag(Constants.Tags.Player);

            foreach (var obj in playerObjects)
            {
                var otherPlayerState = obj.GetComponent<PlayerState>();
                if (otherPlayerState.OwnerClientId == clientId)
                {
                    return otherPlayerState;
                }
            }

            return null;
        }

        public void ShowAlertForItemsAddedToInventory(string alertText)
        {
            GameManager.Instance.SceneBehaviour.MakeAnnouncementClientRpc(alertText, _clientRpcParams);
        }

        public void AlertOfInventoryRemovals(int itemsRemovedCount)
        {
            var message = GameManager.Instance.Localizer.Translate("ui.alert.itemsremoved");
            GameManager.Instance.SceneBehaviour.MakeAnnouncementClientRpc(string.Format(message, itemsRemovedCount), _clientRpcParams);
        }

        public void AlertInventoryIsFull()
        {
            GameManager.Instance.SceneBehaviour.MakeAnnouncementClientRpc(GameManager.Instance.Localizer.Translate("ui.alert.itemsatmax"), _clientRpcParams);
        }

        private void LoadFromPlayerData(PlayerData playerData)
        {
            //Debug.LogError($"Loading player data into PlayerState with OwnerClientId: {OwnerClientId}");

            Username = playerData.Username;
            SetName();

            if (IsServer)
            {
                Health.Value = GetHealthMax();
            }

            try
            {
                Inventory.LoadInventory(playerData.Inventory);
                playerData.InventoryLoadedSuccessfully = true;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.ToString());
                playerData.InventoryLoadedSuccessfully = false;
            }

            if (GameManager.Instance.UserRegistry.PlayerData.ContainsKey(playerData.Username))
            {
                Debug.LogWarning($"Overwriting player data for username '{playerData.Username}'");
                GameManager.Instance.UserRegistry.PlayerData[playerData.Username] = playerData;
            }
            else
            {
                GameManager.Instance.UserRegistry.PlayerData.Add(playerData.Username, playerData);
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
                //Debug.LogError("Trying to set texture before player data is loaded");
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

        public bool SpendMana(Spell activeSpell)
        {
            var manaCost = GetManaCost(activeSpell);

            if (Mana.Value < manaCost)
            {
                return false;
            }

            Mana.Value -= manaCost;

            return true;
        }

        public void SpawnSpellProjectile(Spell activeSpell, Vector3 startPosition, Vector3 direction, ulong senderClientId)
        {
            if (!IsServer)
            {
                Debug.LogWarning("Tried to spawn a projectile spell when not on the server");
            }

            if (!SpendMana(activeSpell))
            {
                return;
            }

            var spellObject = Instantiate(GameManager.Instance.Prefabs.Combat.SpellProjectile, startPosition, Quaternion.identity);

            var spellScript = spellObject.GetComponent<SpellProjectileBehaviour>();
            spellScript.PlayerClientId = senderClientId;
            spellScript.SpellId = activeSpell.Id;
            spellScript.SpellDirection = direction;

            spellObject.GetComponent<NetworkObject>().Spawn(true);

            spellObject.transform.parent = GameManager.Instance.SceneBehaviour.GetTransform();
        }

        public void SpawnSpellSelf(Spell activeSpell, Vector3 startPosition, Vector3 direction, ulong senderClientId)
        {
            if (!IsServer)
            {
                Debug.LogWarning("Tried to spawn a self spell when not on the server");
            }

            if (!SpendMana(activeSpell))
            {
                return;
            }

            var spellObject = Instantiate(GameManager.Instance.Prefabs.Combat.SpellSelf, startPosition, Quaternion.identity);

            var spellScript = spellObject.GetComponent<SpellSelfBehaviour>();
            spellScript.PlayerClientId = senderClientId;
            spellScript.SpellId = activeSpell.Id;
            spellScript.SpellDirection = direction;

            spellObject.GetComponent<NetworkObject>().Spawn(true);

            spellObject.transform.parent = GameManager.Instance.SceneBehaviour.GetTransform();
        }

        public void SpawnSpellWall(Spell activeSpell, Vector3 startPosition, Quaternion rotation, ulong senderClientId)
        {
            if (!IsServer)
            {
                Debug.LogWarning("Tried to spawn a wall spell when not on the server");
            }

            var prefab = GameManager.Instance.Prefabs.Combat.SpellWall;

            var spellObject = Instantiate(prefab, startPosition, rotation);
            GameManager.Instance.SceneBehaviour.GetSpawnService().AdjustPositionToBeAboveGround(startPosition, spellObject);

            var spellScript = spellObject.GetComponent<SpellWallBehaviour>();
            spellScript.PlayerClientId = senderClientId;
            spellScript.SpellId = activeSpell.Id;

            spellObject.GetComponent<NetworkObject>().Spawn(true);

            spellObject.transform.parent = GameManager.Instance.SceneBehaviour.GetTransform();
        }

        public void SpawnSpellZone(Spell activeSpell, Vector3 startPosition, ulong senderClientId)
        {
            if (!IsServer)
            {
                Debug.LogWarning("Tried to spawn a zone spell when not on the server");
            }

            var prefab = GameManager.Instance.Prefabs.Combat.SpellZone;

            var spellObject = Instantiate(prefab, startPosition, Quaternion.identity);
            GameManager.Instance.SceneBehaviour.GetSpawnService().AdjustPositionToBeAboveGround(startPosition, spellObject, false);

            var spellScript = spellObject.GetComponent<SpellZoneBehaviour>();
            spellScript.PlayerClientId = senderClientId;
            spellScript.SpellId = activeSpell.Id;

            spellObject.GetComponent<NetworkObject>().Spawn(true);

            spellObject.transform.parent = GameManager.Instance.SceneBehaviour.GetTransform();
        }

        public void CastSpellTouch(Spell activeSpell, Vector3 startPosition, Vector3 direction, ulong senderClientId)
        {
            if (!IsServer)
            {
                Debug.LogWarning("Tried to cast a touch spell when not on the server");
            }

            //NOTE: Don't call SpendMana() here as it is called in the behaviour
            if (Mana.Value < GetManaCost(activeSpell))
            {
                return;
            }

            // ReSharper disable once ObjectCreationAsStatement
            new SpellTouchBehaviour(activeSpell, startPosition, direction, senderClientId);
        }

        public void ToggleSpellBeam(bool isLeftHand, Spell activeSpell, Vector3? startPosition, Vector3? direction)
        {
            if (isLeftHand && _spellBeingCastLeft != null)
            {
                Destroy(_spellBeingCastLeft);
                _spellBeingCastLeft = null;
                return;
            }

            if (!isLeftHand && _spellBeingCastRight != null)
            {
                Destroy(_spellBeingCastRight);
                _spellBeingCastRight = null;
                return;
            }

            //NOTE: Don't call SpendMana() here as it is called in the behaviour
            if (Mana.Value < GetManaCost(activeSpell))
            {
                return;
            }

            if (!startPosition.HasValue || !direction.HasValue)
            {
                Debug.LogError("Tried to ToggleSpellBeam on without position and direction");
                return;
            }

            //NOTE: Can't parent to PlayerCamera otherwise it doesn't parent at all!
            var spellObject = Instantiate(
                GameManager.Instance.Prefabs.Combat.SpellBeam,
                startPosition.Value,
                Quaternion.LookRotation(direction.Value)
            );

            var spellScript = spellObject.GetComponent<SpellBeamBehaviour>();
            spellScript.SpellId = activeSpell.Id;
            spellScript.IsLeftHand = isLeftHand;

            spellObject.GetComponent<NetworkObject>().Spawn(true);

            spellObject.transform.parent = transform;

            if (isLeftHand)
            {
                _spellBeingCastLeft = spellObject;
            }
            else
            {
                _spellBeingCastRight = spellObject;
            }
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
            if (clientId != null)
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
            PlayerSpawnStateChangeClientRpc(Vector3.zero, LivingEntityState.Dead, killerName, itemName, RpcHelper.ForNearbyPlayers());

            YouDiedClientRpc(_clientRpcParams);

            if (_spellBeingCastLeft != null)
            {
                Destroy(_spellBeingCastLeft);
                _spellBeingCastLeft = null;
            }

            if (_spellBeingCastRight != null)
            {
                Destroy(_spellBeingCastRight);
                _spellBeingCastRight = null;
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

        #endregion

    }
}
