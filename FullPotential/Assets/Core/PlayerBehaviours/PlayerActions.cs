using System.Linq;
using FullPotential.Api.Gameplay.Data;
using FullPotential.Api.Gameplay.Enums;
using FullPotential.Api.Gameplay.Helpers;
using FullPotential.Api.Registry.Loot;
using FullPotential.Api.Unity.Constants;
using FullPotential.Api.Unity.Helpers;
using FullPotential.Core.GameManagement;
using FullPotential.Core.Gameplay.Crafting;
using FullPotential.Core.Gameplay.Data;
using FullPotential.Core.Networking;
using FullPotential.Core.Networking.Data;
using FullPotential.Core.Registry;
using FullPotential.Core.Utilities.UtilityBehaviours;
using TMPro;
using Unity.Netcode;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.PlayerBehaviours
{
    public class PlayerActions : NetworkBehaviour
    {
#pragma warning disable CS0649
        [SerializeField] private Camera _playerCamera;
        [SerializeField] private Camera _inFrontOfPlayerCamera;
        [SerializeField] private GameObject _hitTextPrefab;
#pragma warning restore CS0649

        //Services
        private ResultFactory _resultFactory;

        private bool _hasMenuOpen;
        private MainCanvasObjects _mainCanvasObjects;
        private bool _toggleGameMenu;
        private bool _toggleCharacterMenu;
        private PlayerState _playerState;
        private PlayerMovement _playerMovement;
        private UserRegistry _userRegistry;
        private Interactable _focusedInteractable;
        private Camera _sceneCamera;
        private ClientRpcParams _clientRpcParams;

        private readonly FragmentedMessageReconstructor _inventoryChangesReconstructor = new FragmentedMessageReconstructor();

        #region Event handlers

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            _playerState = GetComponent<PlayerState>();
            _playerMovement = GetComponent<PlayerMovement>();

            _userRegistry = GameManager.Instance.GetService<UserRegistry>();
            _resultFactory = GameManager.Instance.GetService<ResultFactory>();
        }

        // ReSharper disable once UnusedMember.Local
        private void Start()
        {
            if (!IsOwner)
            {
                return;
            }

            _mainCanvasObjects = GameManager.Instance.MainCanvasObjects;

            _mainCanvasObjects.Hud.SetActive(true);

            if (Debug.isDebugBuild)
            {
                _mainCanvasObjects.DebuggingOverlay.SetActive(true);
            }

            _sceneCamera = Camera.main;
            if (_sceneCamera != null)
            {
                _sceneCamera.gameObject.SetActive(false);
            }

            //Avoids weapons clipping with other objects
            _playerState.InFrontOfPlayer.transform.parent = _inFrontOfPlayerCamera.transform;
            GameObjectHelper.SetGameLayerRecursive(_playerState.InFrontOfPlayer, LayerMask.NameToLayer(Layers.InFrontOfPlayer));

            _inFrontOfPlayerCamera.gameObject.SetActive(true);
            _playerCamera.gameObject.SetActive(true);

            if (IsClient)
            {
                Camera.main.fieldOfView = GameManager.Instance.AppOptions.FieldOfView;
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            _clientRpcParams.Send.TargetClientIds = new[] { OwnerClientId };
        }

        // ReSharper disable once UnusedMember.Local
        private void Update()
        {
            CheckForInteractable();
        }

        // ReSharper disable once UnusedMember.Local
        private void FixedUpdate()
        {
            UpdateMenuStates();
        }

        // ReSharper disable once UnusedMember.Local
        private void OnDisable()
        {
            if (!IsOwner)
            {
                return;
            }

            Cursor.lockState = CursorLockMode.None;

            if (_mainCanvasObjects != null && _mainCanvasObjects.Hud != null)
            {
                _mainCanvasObjects.Hud.SetActive(false);
            }

            if (_sceneCamera != null)
            {
                _sceneCamera.gameObject.SetActive(true);
            }
        }

        #endregion

        #region Input Event Handlers
#pragma warning disable IDE0051 // Remove unused private members

        // ReSharper disable once UnusedMember.Local
        private void OnInteract()
        {
            if (_hasMenuOpen)
            {
                return;
            }

            if (_focusedInteractable == null)
            {
                return;
            }

            if (_focusedInteractable.RequiresServerCheck)
            {
                TryToInteractServerRpc(_focusedInteractable.gameObject.name);
            }
            else
            {
                _focusedInteractable.OnInteract(GetComponent<NetworkObject>());
            }
        }

        // ReSharper disable once UnusedMember.Local
        private void OnOpenCharacterMenu()
        {
            _toggleCharacterMenu = true;
        }

        // ReSharper disable once UnusedMember.Local
        private void OnCancel()
        {
            _toggleGameMenu = true;
        }

        // ReSharper disable once UnusedMember.Local
        private void OnAttackLeft()
        {
            OnAttack(true);
        }

        // ReSharper disable once UnusedMember.Local
        private void OnAttackRight()
        {
            OnAttack(false);
        }

        // ReSharper disable once UnusedMember.Local
        private void OnShowCursorStart()
        {
            if (_hasMenuOpen)
            {
                return;
            }

            GameManager.Instance.MainCanvasObjects.DrawingPad.SetActive(true);
            GameManager.Instance.MainCanvasObjects.HudOverlay.ToggleCursorCapture(true);
        }

        // ReSharper disable once UnusedMember.Local
        private void OnShowCursorStop()
        {
            GameManager.Instance.MainCanvasObjects.DrawingPad.SetActive(false);
            GameManager.Instance.MainCanvasObjects.HudOverlay.ToggleCursorCapture(false);
        }

        // ReSharper disable once UnusedMember.Local
        private void OnReloadLeft()
        {
            StartCoroutine(_playerState.ReloadCoroutine(_playerState.HandStatusLeft));
            _playerState.ReloadServerRpc(true);
        }

        // ReSharper disable once UnusedMember.Local
        private void OnReloadRight()
        {
            StartCoroutine(_playerState.ReloadCoroutine(_playerState.HandStatusRight));
            _playerState.ReloadServerRpc(false);
        }

#pragma warning restore IDE0051 // Remove unused private members
        #endregion

        #region ServerRpc calls

        [ServerRpc]
        public void UpdatePlayerSettingsServerRpc(PlayerSettings playerSettings)
        {
            var saveData = _userRegistry.PlayerData[_playerState.Username];
            saveData.Settings = playerSettings ?? new PlayerSettings();
            saveData.IsAsapSaveRequired = true;

            _playerState.TextureUrl.Value = saveData.Settings.TextureUrl;
        }

        [ServerRpc]
        private void TryToInteractServerRpc(string gameObjectName, ServerRpcParams serverRpcParams = default)
        {
            const float searchRadius = 5f;

            var playerNetworkObject = NetworkManager.Singleton.ConnectedClients[serverRpcParams.Receive.SenderClientId].PlayerObject;

            Interactable interactable = null;
            var collidersInRange = Physics.OverlapSphere(playerNetworkObject.transform.position, searchRadius);
            foreach (var colliderNearby in collidersInRange)
            {
                if (colliderNearby.gameObject.name == gameObjectName)
                {
                    var colliderInteractable = colliderNearby.gameObject.GetComponent<Interactable>();

                    if (colliderInteractable == null)
                    {
                        continue;
                    }

                    interactable = colliderInteractable;
                    break;
                }
            }

            if (interactable == null)
            {
                Debug.LogError("Failed to find the interactable with gameObjectName " + gameObjectName);
                return;
            }

            var distance = Vector3.Distance(transform.position, interactable.transform.position);
            if (distance <= interactable.Radius)
            {
                interactable.OnInteract(playerNetworkObject);
            }
        }

        [ServerRpc]
        public void CraftItemServerRpc(string componentIdsCsv, string categoryName, string craftableTypeName, bool isTwoHanded, string itemName)
        {
            var componentIdArray = componentIdsCsv.Split(',');

            var components = _playerState.Inventory.GetComponentsFromIds(componentIdArray);

            if (components.Count != componentIdArray.Length)
            {
                Debug.LogWarning("Someone tried cheating: One or more IDs provided are not in the inventory");
                return;
            }

            var craftedItem = _resultFactory.GetCraftedItem(
                categoryName,
                craftableTypeName,
                isTwoHanded,
                components
            );

            if (_playerState.Inventory.ValidateIsCraftable(componentIdArray, craftedItem).Any())
            {
                Debug.LogWarning("Someone tried cheating: validation was skipped");
                return;
            }

            if (!string.IsNullOrWhiteSpace(itemName))
            {
                craftedItem.Name = itemName;
            }

            var invChange = new InventoryChanges
            {
                IdsToRemove = componentIdArray
            };

            InventoryDataHelper.PopulateInventoryChangesWithItem(invChange, craftedItem);

            ApplyInventoryChanges(invChange);

            if (OwnerClientId != 0)
            {
                foreach (var message in FragmentedMessageReconstructor.GetFragmentedMessages(invChange))
                {
                    ApplyInventoryChangesClientRpc(message, _clientRpcParams);
                }
            }
        }

        [ServerRpc]
        public void ClaimLootServerRpc(string id)
        {
            var skipIdCheck = Debug.isDebugBuild && id == "justgimmieloot";

            if (!skipIdCheck && !_playerState.ClaimLoot(id))
            {
                return;
            }

            var loot = _resultFactory.GetLootDrop();

            var invChange = new InventoryChanges { Loot = new[] { loot as Loot } };

            ApplyInventoryChanges(invChange);

            if (OwnerClientId != 0)
            {
                foreach (var message in FragmentedMessageReconstructor.GetFragmentedMessages(invChange))
                {
                    ApplyInventoryChangesClientRpc(message, _clientRpcParams);
                }
            }
        }

        #endregion

        #region ClientRpc calls

        // ReSharper disable once UnusedParameter.Local
        [ClientRpc]
        private void ApplyInventoryChangesClientRpc(string fragmentedMessageJson, ClientRpcParams clientRpcParams)
        {
            var fragmentedMessage = JsonUtility.FromJson<FragmentedMessage>(fragmentedMessageJson);

            _inventoryChangesReconstructor.AddMessage(fragmentedMessage);
            if (!_inventoryChangesReconstructor.HaveAllMessages(fragmentedMessage.GroupId))
            {
                return;
            }

            var changes = JsonUtility.FromJson<InventoryChanges>(_inventoryChangesReconstructor.Reconstruct(fragmentedMessage.GroupId));
            ApplyInventoryChanges(changes);
        }

        // ReSharper disable once UnusedParameter.Global
        [ClientRpc]
        public void ShowDamageClientRpc(Vector3 position, string damage, ClientRpcParams clientRpcParams)
        {
            var offsetX = (float)AttributeCalculator.Random.Next(-9, 10) / 100;
            var offsetY = (float)AttributeCalculator.Random.Next(-9, 10) / 100;
            var offsetZ = (float)AttributeCalculator.Random.Next(-9, 10) / 100;
            var adjustedPosition = position + new Vector3(offsetX, offsetY, offsetZ);
            ShowDamage(adjustedPosition, damage);
        }

        #endregion

        private void UpdateMenuStates()
        {
            if (_toggleGameMenu || _toggleCharacterMenu)
            {
                if (_hasMenuOpen)
                {
                    _mainCanvasObjects.HideAllMenus();
                }
                else if (_toggleGameMenu)
                {
                    _mainCanvasObjects.HideOthersOpenThis(_mainCanvasObjects.EscMenu);
                }
                else if (_toggleCharacterMenu)
                {
                    _mainCanvasObjects.HideOthersOpenThis(_mainCanvasObjects.CharacterMenu);
                }

                Tooltips.HideTooltip();

                _toggleGameMenu = false;
                _toggleCharacterMenu = false;
            }

            _hasMenuOpen = _mainCanvasObjects.IsAnyMenuOpen();
            _playerMovement.enabled = !_hasMenuOpen;

            if (_hasMenuOpen)
            {
                if (Cursor.lockState != CursorLockMode.None)
                {
                    Cursor.lockState = CursorLockMode.None;
                }
            }
            else if (Cursor.lockState != CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        private void ApplyInventoryChanges(InventoryChanges changes)
        {
            ((PlayerInventory)_playerState.Inventory).ApplyInventoryChanges(changes);

            if (changes.IdsToRemove != null && changes.IdsToRemove.Length > 0)
            {
                RefreshCraftingWindow();
            }
        }

        private Ray GetLookDirectionRay()
        {
            return _playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
        }

        private void CheckForInteractable()
        {
            if (Physics.Raycast(GetLookDirectionRay(), out var hit, maxDistance: 1000))
            {
                var interactable = hit.collider.GetComponent<Interactable>();
                if (interactable != null)
                {
                    var distance = Vector3.Distance(_playerCamera.transform.position, interactable.transform.position);
                    if (distance <= interactable.Radius)
                    {
                        if (interactable != _focusedInteractable)
                        {
                            if (_focusedInteractable != null)
                            {
                                _focusedInteractable.OnBlur();
                            }
                            _focusedInteractable = interactable;
                            _focusedInteractable.OnFocus();
                        }

                        return;
                    }
                }
            }

            if (_focusedInteractable != null)
            {
                _focusedInteractable.OnBlur();
                _focusedInteractable = null;
            }
        }

        private void OnAttack(bool isLeftHand)
        {
            if (_hasMenuOpen || _playerState.AliveState != LivingEntityState.Alive)
            {
                return;
            }

            var handPosition = isLeftHand
                ? _playerState.Positions.LeftHandInFront.position
                : _playerState.Positions.RightHandInFront.position;

            if (IsServer || _playerState.TryToAttack(isLeftHand))
            {
                _playerState.TryToAttackServerRpc(isLeftHand);
            }
        }

        private static void RefreshCraftingWindow()
        {
            var craftingUi = GameManager.Instance.MainCanvasObjects.GetCharacterMenuUiCraftingTab();

            if (craftingUi.gameObject.activeSelf)
            {
                craftingUi.ResetUi();
            }
        }

        public void ShowDamage(Vector3 position, string damage)
        {
            var hit = Instantiate(_hitTextPrefab);
            hit.transform.SetParent(GameManager.Instance.MainCanvasObjects.HitNumberContainer.transform, false);
            hit.SetActive(true);

            var hitText = hit.GetComponent<TextMeshProUGUI>();
            hitText.text = damage;

            const int maxDistanceForMinFontSize = 40;
            var distance = Vector3.Distance(Camera.main.transform.position, position);
            var fontSize = maxDistanceForMinFontSize - distance;
            if (fontSize < hitText.fontSizeMin) { fontSize = hitText.fontSizeMin; }
            else if (fontSize > hitText.fontSizeMax) { fontSize = hitText.fontSizeMax; }
            hitText.fontSize = fontSize;

            var sticky = hit.GetComponent<StickUiToWorldPosition>();
            sticky.WorldPosition = position;

            Destroy(hit, 1f);
        }

    }
}
