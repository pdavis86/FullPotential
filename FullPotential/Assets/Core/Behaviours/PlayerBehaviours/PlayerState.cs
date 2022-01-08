using FullPotential.Api.Behaviours;
using FullPotential.Core.Behaviours.GameManagement;
using FullPotential.Core.Behaviours.SpellBehaviours;
using FullPotential.Core.Data;
using FullPotential.Core.Networking;
using FullPotential.Core.Registry.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FullPotential.Core.Behaviours.Environment;
using FullPotential.Core.Behaviours.Ui;
using FullPotential.Core.Extensions;
using FullPotential.Core.Helpers;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
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
        public PositionTransforms Positions;
        [SerializeField] private TextMeshProUGUI _nameTag;
        [SerializeField] private MeshRenderer _mainMesh;
        [SerializeField] private MeshRenderer _leftMesh;
        [SerializeField] private MeshRenderer _rightMesh;
        [SerializeField] private Slider _healthSlider;
#pragma warning restore 0649

        public GameObject InFrontOfPlayer;
        public GameObject PlayerCamera;

        public readonly NetworkVariable<int> Health = new NetworkVariable<int>(100);

        [HideInInspector] public bool IsDead;
        [HideInInspector] public PlayerInventory Inventory;
        [HideInInspector] public string PlayerToken;
        [HideInInspector] public readonly NetworkVariable<FixedString512Bytes> TextureUrl = new NetworkVariable<FixedString512Bytes>();

        private Rigidbody _rb;
        private PlayerActions _playerActions;
        private ClientRpcParams _clientRpcParams;
        private GameObject _spellBeingCastLeft;
        private GameObject _spellBeingCastRight;
        private bool _loadWasSuccessful;
        private GameObject _graphicsGameObject;
        private readonly Dictionary<ulong, long> _damageTaken = new Dictionary<ulong, long>();

        private string _username;

        private FragmentedMessageReconstructor _loadPlayerDataReconstructor = new FragmentedMessageReconstructor();
        private readonly Dictionary<string, DateTime> _unclaimedLoot = new Dictionary<string, DateTime>();

        #region Event handlers

        private void Awake()
        {
            TextureUrl.OnValueChanged += OnTextureChanged;
            Health.OnValueChanged += OnHealthChanged;

            Inventory = GetComponent<PlayerInventory>();
            _rb = GetComponent<Rigidbody>();
            _playerActions = GetComponent<PlayerActions>();
            _graphicsGameObject = transform.Find("Graphics").gameObject;
        }

        private void Start()
        {
            if (IsOwner)
            {
                GameObjectHelper.GetObjectAtRoot(Constants.GameObjectNames.SceneCanvas).SetActive(false);
                GameManager.Instance.DataStore.LocalPlayer = gameObject;
                _nameTag.gameObject.SetActive(false);
                _healthSlider.gameObject.SetActive(false);

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
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            _clientRpcParams.Send.TargetClientIds = new[] { OwnerClientId };
        }

        private void OnTextureChanged(FixedString512Bytes previousValue, FixedString512Bytes newValue)
        {
            if (!string.IsNullOrWhiteSpace(TextureUrl.Value.ToString()))
            {
                StartCoroutine(SetTexture());
            }
        }

        private void OnHealthChanged(int previousValue, int newValue)
        {
            if (!IsOwner)
            {
                _healthSlider.value = (float)newValue / GetHealthMax();
            }

            if (NetworkManager.LocalClientId != OwnerClientId)
            {
                return;
            }

            GameManager.Instance.MainCanvasObjects.Hud.GetComponent<Hud>().UpdateHealthPercentage((float)newValue / GetHealthMax());
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
        public void UpdatePositionsAndRotationsServerRpc(Vector3 rbPosition, Quaternion rbRotation, Vector3 rbVelocity, Quaternion cameraRotation)
        {
            //NOTE: This does not stop players cheating their position. That's a problem for another day. Also sends data to ALL clients
            UpdatePositionsAndRotations(rbPosition, rbRotation, rbVelocity, cameraRotation);
            UpdatePositionsAndRotationsClientRpc(rbPosition, rbRotation, rbVelocity, cameraRotation, new ClientRpcParams());
        }

        [ServerRpc]
        public void RespawnServerRpc()
        {
            Health.Value = GetHealthMax();
            var spawnPoint = GameManager.Instance.SceneBehaviour.GetSpawnPoint(gameObject);
            RespawnClientRpc(_clientRpcParams);

            //NOTE: Sent to all players
            PlayerRespawnClientRpc(spawnPoint.Position, spawnPoint.Rotation, false, new ClientRpcParams());

            IsDead = false;
        }

        #endregion

        #region ClientRpc calls

        // ReSharper disable once UnusedParameter.Global
        [ClientRpc]
        public void ShowAlertClientRpc(string alertText, ClientRpcParams clientRpcParams)
        {
            _playerActions.ShowAlert(alertText);
        }

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

            SetNameTag();

            if (!string.IsNullOrWhiteSpace(TextureUrl.Value.ToString()))
            {
                StartCoroutine(SetTexture());
            }
        }

        // ReSharper disable once UnusedParameter.Global
        [ClientRpc]
        public void UpdatePositionsAndRotationsClientRpc(Vector3 rbPosition, Quaternion rbRotation, Vector3 rbVelocity, Quaternion cameraRotation, ClientRpcParams clientRpcParams)
        {
            if (!IsOwner)
            {
                UpdatePositionsAndRotations(rbPosition, rbRotation, rbVelocity, cameraRotation);
            }
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

        //todo: make a list of commands to run in reverse order when necessary. Delegates for dead and alive

        // ReSharper disable once UnusedParameter.Global
        [ClientRpc]
        public void YouDiedClientRpc(ClientRpcParams clientRpcParams)
        {
            GameManager.Instance.MainCanvasObjects.HideAllMenus();

            IsDead = true;

            foreach (var comp in _behavioursForRespawn)
            {
                comp.enabled = false;
            }

            GameObjectHelper.GetObjectAtRoot(Constants.GameObjectNames.SceneCamera).SetActive(true);

            MainCanvasObjects.Instance.Hud.SetActive(false);
            MainCanvasObjects.Instance.Respawn.SetActive(true);
        }

        // ReSharper disable once UnusedParameter.Global
        [ClientRpc]
        public void PlayerRespawnClientRpc(Vector3 position, Quaternion rotation, bool isDead, ClientRpcParams clientRpcParams)
        {
            if (!isDead)
            {
                UpdatePositionsAndRotations(position, rotation, Vector3.zero, null);
            }

            _graphicsGameObject.gameObject.SetActive(!isDead);
            _rb.useGravity = !isDead;
            _rb.isKinematic = isDead;
            GetComponent<Collider>().enabled = !isDead;
        }

        // ReSharper disable once UnusedParameter.Global
        [ClientRpc]
        public void RespawnClientRpc(ClientRpcParams clientRpcParams)
        {
            MainCanvasObjects.Instance.Respawn.SetActive(false);
            MainCanvasObjects.Instance.Hud.SetActive(true);

            GameObjectHelper.GetObjectAtRoot(Constants.GameObjectNames.SceneCamera).SetActive(false);

            foreach (var comp in _behavioursForRespawn)
            {
                comp.enabled = true;
            }

            IsDead = false;
        }

        #endregion

        private void GetAndLoadPlayerData(bool reduced, ulong? clientId)
        {
            //Debug.Log($"Loading player data for {OwnerClientId}, reduced: {reduced}");

            var playerData = GameManager.Instance.UserRegistry.Load(PlayerToken, null, reduced);

            if (clientId.HasValue)
            {
                //Don#'t send data to the server. It already has it loaded
                if (clientId.Value == 0) { return; }

                StartCoroutine(LoadFromPlayerDataCoroutine(playerData, clientId.Value));
            }
            else
            {
                TextureUrl.Value = playerData.Options.TextureUrl ?? string.Empty;
                LoadFromPlayerData(playerData);
            }
        }

        //Need this to get over the key not found exception caused by too many RPC calls with large payloads
        private IEnumerator LoadFromPlayerDataCoroutine(PlayerData playerData, ulong clientId)
        {
            var clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = new[] { clientId } }
            };

            //Debug.LogError($"Sending LoadFromPlayerData messages to client {clientId}");

            foreach (var message in MessageHelper.GetFragmentedMessages(playerData))
            {
                LoadPlayerDataClientRpc(message, clientRpcParams);
                yield return null;
            }
        }

        private void UpdatePositionsAndRotations(Vector3 rbPosition, Quaternion rbRotation, Vector3 rbVelocity, Quaternion? cameraRotation)
        {
            _rb.position = rbPosition;
            _rb.rotation = rbRotation;
            _rb.velocity = rbVelocity;

            if (cameraRotation.HasValue)
            {
                PlayerCamera.transform.rotation = cameraRotation.Value;
            }
            else
            {
                PlayerCamera.transform.localEulerAngles = Vector3.zero;
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
            ShowAlertClientRpc(alertText, _clientRpcParams);
        }

        public void AlertOfInventoryRemovals(int itemsRemoved)
        {
            ShowAlertClientRpc($"Removed {itemsRemoved} items from the inventory after handling message on " + (IsServer ? "server" : "client") + " for " + gameObject.name, _clientRpcParams);
        }

        public void AlertInventoryIsFull()
        {
            ShowAlertClientRpc("Your inventory is at max", _clientRpcParams);
        }

        private void LoadFromPlayerData(PlayerData playerData)
        {
            //Debug.LogError($"Loading player data into PlayerState with OwnerClientId: {OwnerClientId}");

            _username = playerData.Username;
            SetNameTag();

            if (IsServer)
            {
                Health.Value = GetHealthMax();
            }

            try
            {
                Inventory.LoadInventory(playerData.Inventory);
                _loadWasSuccessful = true;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.ToString());
                _loadWasSuccessful = false;
            }
        }

        private void SetNameTag()
        {
            if (IsOwner)
            {
                return;
            }

            _nameTag.text = string.IsNullOrWhiteSpace(_username)
                ? "Player " + NetworkObjectId
                : _username;
        }

        private IEnumerator SetTexture()
        {
            var textureUrl = TextureUrl.Value.ToString().ToLower();

            string filePath;
            if (textureUrl.StartsWith("http"))
            {
                filePath = Application.persistentDataPath + "/" + _username + ".png";

                // ReSharper disable once StringLiteralTypo
                var validatePath = Application.persistentDataPath + "/" + _username + ".skinvalidate";

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

                        if (webRequest.downloadHandler.data != null)
                        {
                            System.IO.File.WriteAllBytes(filePath, webRequest.downloadHandler.data);
                            System.IO.File.WriteAllText(validatePath, textureUrl);
                        }
                    }
                }
            }
            else
            {
                filePath = textureUrl;
            }

            if (!System.IO.File.Exists(filePath))
            {
                Debug.LogWarning("Not applying player texture because the file does not exist");
            }
            else
            {
                var tex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
                tex.LoadImage(System.IO.File.ReadAllBytes(filePath));
                var newMat = new Material(_mainMesh.material.shader)
                {
                    mainTexture = tex
                };

                _mainMesh.material = newMat;
                _leftMesh.material = newMat;
                _rightMesh.material = newMat;
            }
        }

        public void Save()
        {
            if (!IsServer)
            {
                Debug.LogError("Tried to save when not on the server");
            }

            //Debug.Log("Saving player data for " + gameObject.name);

            if (!_loadWasSuccessful)
            {
                Debug.LogWarning("Not saving because the load failed");
                return;
            }

            var saveData = new PlayerData
            {
                Username = _username,
                Options = new PlayerOptions
                {
                    TextureUrl = TextureUrl.Value.ToString()
                },
                Inventory = Inventory.GetSaveData()
            };

            GameManager.Instance.UserRegistry.Save(saveData);
        }

        public void SpawnSpellProjectile(Spell activeSpell, Vector3 startPosition, Vector3 direction, ulong senderClientId)
        {
            if (!IsServer)
            {
                Debug.LogWarning("Tried to spawn a projectile spell when not on the server");
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

            // ReSharper disable once ObjectCreationAsStatement
            new SpellTouchBehaviour(activeSpell, startPosition, direction, senderClientId);
        }

        public void ToggleSpellBeam(bool isLeftHand, Spell activeSpell, Vector3 startPosition, Vector3 direction)
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

            //NOTE: Can't parent to PlayerCamera otherwise it doesn't parent at all!
            var spellObject = Instantiate(
                GameManager.Instance.Prefabs.Combat.SpellBeam,
                startPosition,
                Quaternion.LookRotation(direction)
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

        public int GetHealthMax()
        {
            return 100;
        }

        public int GetHealth()
        {
            return Health.Value;
        }

        public void TakeDamage(ulong? clientId, int amount)
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
        }

        public void HandleDeath()
        {
            IsDead = true;

            foreach (var item in _damageTaken)
            {
                var playerState = NetworkManager.Singleton.ConnectedClients[item.Key].PlayerObject.gameObject.GetComponent<PlayerState>();
                playerState.SpawnLootChest(transform.position);
            }

            _damageTaken.Clear();

            //NOTE: Sent to all players
            PlayerRespawnClientRpc(Vector3.zero, Quaternion.identity, true, new ClientRpcParams());

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

        #region Nested Classes

        [Serializable]
        public struct PositionTransforms
        {
            public Transform LeftHandInFront;
            public Transform RightHandInFront;
        }

        #endregion

    }
}
