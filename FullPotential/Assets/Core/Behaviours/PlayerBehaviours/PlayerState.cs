﻿using FullPotential.Api.Behaviours;
using FullPotential.Core.Behaviours.GameManagement;
using FullPotential.Core.Behaviours.SpellBehaviours;
using FullPotential.Core.Data;
using FullPotential.Core.Networking;
using FullPotential.Core.Registry.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using FullPotential.Core.Behaviours.Environment;
using FullPotential.Core.Behaviours.Ui;
using FullPotential.Core.Extensions;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Networking;

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
        public PositionTransforms Positions;
        [SerializeField] private TextMeshProUGUI _nameTag;
        [SerializeField] private MeshRenderer _mainMesh;
        [SerializeField] private MeshRenderer _leftMesh;
        [SerializeField] private MeshRenderer _rightMesh;
#pragma warning restore 0649

        public PlayerInventory Inventory;
        public GameObject InFrontOfPlayer;
        public GameObject PlayerCamera;

        [HideInInspector] public string PlayerToken;
        [HideInInspector] public readonly NetworkVariable<FixedString512Bytes> TextureUrl = new NetworkVariable<FixedString512Bytes>();
        public readonly NetworkVariable<int> Health = new NetworkVariable<int>(100);

        private string _username;
        private Rigidbody _rb;
        private PlayerActions _playerActions;
        private ClientRpcParams _clientRpcParams;
        private GameObject _spellBeingCastLeft;
        private GameObject _spellBeingCastRight;
        private bool _loadWasSuccessful;

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
        }

        private void Start()
        {
            if (IsOwner)
            {
                GameManager.Instance.DataStore.LocalPlayer = gameObject;
            }
            else
            {
                foreach (var comp in _behavioursToDisable)
                {
                    comp.enabled = false;
                }
            }

            gameObject.name = "Player ID " + NetworkObjectId;

            _clientRpcParams.Send.TargetClientIds = new[] { OwnerClientId };

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

        // ReSharper disable UnusedParameter.Local
        private void OnUsernameChanged(FixedString64Bytes previousValue, FixedString64Bytes newValue)
        {
            SetNameTag();
        }
        // ReSharper restore UnusedParameter.Local

        private void OnTextureChanged(FixedString512Bytes previousValue, FixedString512Bytes newValue)
        {
            if (!string.IsNullOrWhiteSpace(TextureUrl.Value.ToString()))
            {
                StartCoroutine(SetTexture());
            }
        }

        private void OnHealthChanged(int previousValue, int newValue)
        {
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
            //NOTE: This does not stop players cheating their position. That's a problem for another day
            UpdatePositionsAndRotations(rbPosition, rbRotation, rbVelocity, cameraRotation);
            UpdatePositionsAndRotationsClientRpc(rbPosition, rbRotation, rbVelocity, cameraRotation);
        }

        [ServerRpc]
        public void ClaimLootServerRpc(string id)
        {
            if (!_unclaimedLoot.ContainsKey(id))
            {
                Debug.LogError($"Could not find loot with ID {id}");
            }

            _unclaimedLoot.Remove(id);

            var loot = GameManager.Instance.ResultFactory.GetLootDrop();
            var invChange = new InventoryChanges { Loot = new[] { loot as Loot } };
            Inventory.ApplyInventoryChanges(invChange);
        }

        #endregion

        #region ClientRpc calls

        // ReSharper disable once UnusedParameter.Global
        [ClientRpc]
        public void ShowAlertClientRpc(string alertText, ClientRpcParams clientRpcParams = default)
        {
            _playerActions.ShowAlert(alertText);
        }

        // ReSharper disable once UnusedParameter.Global
        [ClientRpc]
        public void ShowDamageClientRpc(Vector3 position, string damage, ClientRpcParams clientRpcParams = default)
        {
            _playerActions.ShowDamage(position, damage);
        }

        // ReSharper disable once UnusedParameter.Global
        [ClientRpc]
        public void LoadPlayerDataClientRpc(string fragmentedMessageJson, ClientRpcParams clientRpcParams = default)
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
        public void UpdatePositionsAndRotationsClientRpc(Vector3 rbPosition, Quaternion rbRotation, Vector3 rbVelocity, Quaternion cameraRotation, ClientRpcParams clientRpcParams = default)
        {
            if (!IsOwner)
            {
                UpdatePositionsAndRotations(rbPosition, rbRotation, rbVelocity, cameraRotation);
            }
        }

        // ReSharper disable once UnusedParameter.Global
        [ClientRpc]
        public void SpawnLootChestClientRpc(string id, Vector3 position, ClientRpcParams clientRpcParams = default)
        {
            var prefab = GameManager.Instance.Prefabs.Environment.LootChest;

            var go = Instantiate(prefab, position, transform.rotation * Quaternion.Euler(0, 90, 0));

            GameManager.Instance.SceneBehaviour.GetSpawnService().AdjustPositionToBeAboveGround(position, go, false);

            go.transform.parent = GameManager.Instance.SceneBehaviour.GetTransform();
            go.name = id;

            var lootScript = go.GetComponent<LootInteractable>();
            lootScript.UnclaimedLootId = id;
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
                TextureUrl.Value = playerData.Options?.TextureUrl;
                LoadFromPlayerData(playerData);
            }
        }

        //Need this to get over the key not found exception caused by too many RPC calls with large payloads
        private IEnumerator LoadFromPlayerDataCoroutine(PlayerData playerData, ulong clientId)
        {
            var clientRpcParams = new ClientRpcParams();
            clientRpcParams.Send.TargetClientIds = new[] { clientId };

            //Debug.LogError($"Sending LoadFromPlayerData messages to client {clientId}");

            foreach (var message in MessageHelper.GetFragmentedMessages(playerData))
            {
                LoadPlayerDataClientRpc(message, clientRpcParams);
                yield return null;
            }
        }

        private void UpdatePositionsAndRotations(Vector3 rbPosition, Quaternion rbRotation, Vector3 rbVelocity, Quaternion cameraRotation)
        {
            _rb.position = rbPosition;
            _rb.rotation = rbRotation;
            _rb.velocity = rbVelocity;
            PlayerCamera.transform.rotation = cameraRotation;
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
                _nameTag.text = null;
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
                        System.IO.File.WriteAllBytes(filePath, webRequest.downloadHandler.data);
                    }
                    System.IO.File.WriteAllText(validatePath, textureUrl);
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
            spellScript.PlayerClientId.Value = senderClientId;
            spellScript.SpellId.Value = activeSpell.Id;
            spellScript.SpellDirection.Value = direction;

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
            //todo: do this properly
            return 100;
        }

        public int GetHealth()
        {
            return Health.Value;
        }

        public void TakeDamage(ulong? clientId, int amount)
        {
            //todo: implement player TakeDamage()
        }

        public void HandleDeath()
        {
            //todo: implement player HandleDeath()
        }

        public void SpawnLootChest(Vector3 position)
        {
            //todo: clean out expired loot

            var id = Guid.NewGuid().ToMinimisedString();

            _unclaimedLoot.Add(id, DateTime.UtcNow.AddHours(1));

            var clientRpcParams = new ClientRpcParams();
            clientRpcParams.Send.TargetClientIds = new[] { OwnerClientId };
            SpawnLootChestClientRpc(id, position, clientRpcParams);
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
