using FullPotential.Api.Behaviours;
using FullPotential.Core.Behaviours.GameManagement;
using FullPotential.Core.Behaviours.SpellBehaviours;
using FullPotential.Core.Data;
using FullPotential.Core.Networking;
using FullPotential.Core.Registry.Types;
using System;
using System.Collections;
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
        private readonly NetworkVariable<FixedString64Bytes> _username = new NetworkVariable<FixedString64Bytes>();
        [HideInInspector] public readonly NetworkVariable<FixedString512Bytes> TextureUrl = new NetworkVariable<FixedString512Bytes>();
        public readonly NetworkVariable<int> Health = new NetworkVariable<int>();

        private Rigidbody _rb;
        private PlayerActions _playerActions;
        private ClientRpcParams _clientRpcParams;
        private GameObject _spellBeingCastLeft;
        private GameObject _spellBeingCastRight;
        private FragmentedMessageReconstructor _loadPlayerDataReconstructor = new FragmentedMessageReconstructor();
        private bool _loadWasSuccessful;

        #region Event handlers

        private void Awake()
        {
            _username.OnValueChanged += OnUsernameChanged;
            TextureUrl.OnValueChanged += OnTextureChanged;

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
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsOwner)
            {
                RequestPlayerDataServerRpc();
            }
        }

        private void OnDisable()
        {
            _username.OnValueChanged -= OnUsernameChanged;
            TextureUrl.OnValueChanged -= OnTextureChanged;
        }

        private void OnUsernameChanged(FixedString64Bytes previousValue, FixedString64Bytes newValue)
        {
            SetNameTag();
        }

        private void OnTextureChanged(FixedString512Bytes previousValue, FixedString512Bytes newValue)
        {
            if (!string.IsNullOrWhiteSpace(TextureUrl.Value.ToString()))
            {
                StartCoroutine(SetTexture());
            }
        }

        #endregion

        #region ServerRpc calls

        [ServerRpc]
        public void RequestPlayerDataServerRpc()
        {
            var playerData = Registry.UserRegistry.Load(PlayerToken, null);

            if (OwnerClientId != 0)
            {
                LoadFromPlayerData(playerData);
            }

            foreach (var message in MessageHelper.GetFragmentedMessages(JsonUtility.ToJson(playerData)))
            {
                LoadPlayerDataClientRpc(JsonUtility.ToJson(message), _clientRpcParams);
            }
        }

        [ServerRpc]
        public void UpdatePositionsAndRotationsServerRpc(Vector3 rbPosition, Quaternion rbRotation, Quaternion cameraRotation)
        {
            //NOTE: This does not stop players cheating their position. That's a problem for another day
            UpdatePositionsAndRotations(rbPosition, rbRotation, cameraRotation);
            UpdatePositionsAndRotationsClientRpc(rbPosition, rbRotation, cameraRotation);
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

            _loadPlayerDataReconstructor.AddMessage(fragmentedMessage);
            if (!_loadPlayerDataReconstructor.HaveAllMessages(fragmentedMessage.GroupId))
            {
                return;
            }

            var playerData = JsonUtility.FromJson<PlayerData>(_loadPlayerDataReconstructor.Reconstruct(fragmentedMessage.GroupId));
            LoadFromPlayerData(playerData);

            _loadPlayerDataReconstructor = null;
        }

        // ReSharper disable once UnusedParameter.Global
        [ClientRpc]
        public void UpdatePositionsAndRotationsClientRpc(Vector3 rbPosition, Quaternion rbRotation, Quaternion cameraRotation, ClientRpcParams clientRpcParams = default)
        {
            if (!IsOwner)
            {
                UpdatePositionsAndRotations(rbPosition, rbRotation, cameraRotation);
            }
        }

        #endregion

        private void UpdatePositionsAndRotations(Vector3 rbPosition, Quaternion rbRotation, Quaternion cameraRotation)
        {
            _rb.position = rbPosition;
            _rb.rotation = rbRotation;
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
            _username.Value = playerData.Username;
            TextureUrl.Value = playerData.Options?.TextureUrl;

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

            var username = _username.Value.ToString();
            _nameTag.text = string.IsNullOrWhiteSpace(username)
                ? "Player " + NetworkObjectId
                : username;
        }

        private IEnumerator SetTexture()
        {
            var textureUrl = TextureUrl.Value.ToString().ToLower();

            string filePath;
            if (textureUrl.StartsWith("http"))
            {
                filePath = Application.persistentDataPath + "/" + _username.Value + ".png";

                var doDownload = true;
                var validatePath = Application.persistentDataPath + "/" + _username.Value + ".skinvalidate";
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
                Username = _username.Value.ToString(),
                Options = new PlayerOptions
                {
                    TextureUrl = TextureUrl.Value.ToString()
                },
                Inventory = Inventory.GetSaveData()
            };

            Registry.UserRegistry.Save(saveData);
        }

        public void SpawnSpellProjectile(Spell activeSpell, Vector3 startPosition, Vector3 direction, ulong senderClientId)
        {
            if (!IsServer)
            {
                Debug.LogWarning("Tried to spawn a projectile spell when not on the server");
            }

            var spellObject = Instantiate(GameManager.Instance.Prefabs.Combat.SpellProjectile, startPosition, Quaternion.identity, GameManager.Instance.RuntimeObjectsContainer);

            var spellScript = spellObject.GetComponent<SpellProjectileBehaviour>();
            spellScript.PlayerClientId.Value = senderClientId;
            spellScript.SpellId.Value = activeSpell.Id;
            spellScript.SpellDirection.Value = direction;

            spellObject.GetComponent<NetworkObject>().Spawn(true);
        }

        public void SpawnSpellSelf(Spell activeSpell, Vector3 startPosition, Vector3 direction, ulong senderClientId)
        {
            if (!IsServer)
            {
                Debug.LogWarning("Tried to spawn a self spell when not on the server");
            }

            var spellObject = Instantiate(GameManager.Instance.Prefabs.Combat.SpellSelf, startPosition, Quaternion.identity, GameManager.Instance.RuntimeObjectsContainer);

            var spellScript = spellObject.GetComponent<SpellSelfBehaviour>();
            spellScript.PlayerClientId.Value = senderClientId;
            spellScript.SpellId.Value = activeSpell.Id;
            spellScript.SpellDirection.Value = direction;

            spellObject.GetComponent<NetworkObject>().Spawn(true);
        }

        public void SpawnSpellWall(Spell activeSpell, Vector3 startPosition, Quaternion rotation, ulong senderClientId)
        {
            if (!IsServer)
            {
                Debug.LogWarning("Tried to spawn a wall spell when not on the server");
            }

            var prefab = GameManager.Instance.Prefabs.Combat.SpellWall;
            var startPositionAdjusted = startPosition + new Vector3(0, prefab.transform.localScale.y / 2);
            var spellObject = Instantiate(prefab, startPositionAdjusted, rotation, GameManager.Instance.RuntimeObjectsContainer);

            var spellScript = spellObject.GetComponent<SpellWallBehaviour>();
            spellScript.PlayerClientId.Value = senderClientId;
            spellScript.SpellId.Value = activeSpell.Id;

            spellObject.GetComponent<NetworkObject>().Spawn(true);
        }

        public void SpawnSpellZone(Spell activeSpell, Vector3 startPosition, ulong senderClientId)
        {
            if (!IsServer)
            {
                Debug.LogWarning("Tried to spawn a zone spell when not on the server");
            }

            var prefab = GameManager.Instance.Prefabs.Combat.SpellZone;
            var startPositionAdjusted = startPosition + new Vector3(0, prefab.transform.localScale.y / 2);
            var spellObject = Instantiate(prefab, startPositionAdjusted, Quaternion.identity, GameManager.Instance.RuntimeObjectsContainer);

            var spellScript = spellObject.GetComponent<SpellZoneBehaviour>();
            spellScript.PlayerClientId.Value = senderClientId;
            spellScript.SpellId.Value = activeSpell.Id;

            spellObject.GetComponent<NetworkObject>().Spawn(true);
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

        public void ToggleSpellBeam(bool isLeftHand, Spell activeSpell, Vector3 startPosition, Vector3 direction, ulong senderClientId)
        {
            if (isLeftHand && _spellBeingCastLeft != null)
            {
                Destroy(_spellBeingCastLeft);
                _spellBeingCastLeft = null;
                return;
            }
            else if (!isLeftHand && _spellBeingCastRight != null)
            {
                Destroy(_spellBeingCastRight);
                _spellBeingCastRight = null;
                return;
            }

            //NOTE: Can't parent to PlayerCamera otherwise it doesn't parent at all!
            var spellObject = Instantiate(
                GameManager.Instance.Prefabs.Combat.SpellBeam,
                startPosition,
                Quaternion.LookRotation(direction),
                transform
            );

            var spellScript = spellObject.GetComponent<SpellBeamBehaviour>();
            spellScript.PlayerClientId.Value = senderClientId;
            spellScript.SpellId.Value = activeSpell.Id;
            spellScript.IsLeftHand.Value = isLeftHand;

            spellObject.GetComponent<NetworkObject>().Spawn(true);

            if (isLeftHand)
            {
                _spellBeingCastLeft = spellObject;
            }
            else
            {
                _spellBeingCastRight = spellObject;
            }
        }

        public void TakeDamage(int amount)
        {
            //todo: implement player TakeDamage()
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
