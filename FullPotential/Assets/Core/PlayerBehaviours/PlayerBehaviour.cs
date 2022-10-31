using System;
using System.Linq;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Gameplay.Data;
using FullPotential.Api.Gameplay.Enums;
using FullPotential.Api.Registry.Loot;
using FullPotential.Api.Unity.Constants;
using FullPotential.Api.Unity.Helpers;
using FullPotential.Core.GameManagement;
using FullPotential.Core.Gameplay.Crafting;
using FullPotential.Core.Gameplay.Tooltips;
using FullPotential.Core.Networking;
using FullPotential.Core.Networking.Data;
using FullPotential.Core.Utilities.Extensions;
using FullPotential.Core.Utilities.UtilityBehaviours;
using TMPro;
using Unity.Netcode;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.PlayerBehaviours
{
    public class PlayerBehaviour : NetworkBehaviour, IPlayerBehaviour
    {
#pragma warning disable 0649
        [SerializeField] private Camera _playerCamera;
        [SerializeField] private Camera _inFrontOfPlayerCamera;
        [SerializeField] private GameObject _hitTextPrefab;
#pragma warning restore 0649

        //Services
        private IResultFactory _resultFactory;
        private IInventoryDataService _inventoryDataService;

        private bool _hasMenuOpen;
        private UserInterface _userInterface;
        private bool _toggleGameMenu;
        private bool _toggleCharacterMenu;
        private PlayerState _playerState;
        private PlayerMovement _playerMovement;
        private Interactable _focusedInteractable;
        private Camera _sceneCamera;
        private ClientRpcParams _clientRpcParams;

        private readonly FragmentedMessageReconstructor _inventoryChangesReconstructor = new FragmentedMessageReconstructor();

        #region Unity Event handlers

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            _playerState = GetComponent<PlayerState>();
            _playerMovement = GetComponent<PlayerMovement>();

            _resultFactory = GameManager.Instance.GetService<IResultFactory>();
            _inventoryDataService = GameManager.Instance.GetService<IInventoryDataService>();
        }

        // ReSharper disable once UnusedMember.Local
        private void Start()
        {
            if (!IsOwner)
            {
                return;
            }

            _userInterface = GameManager.Instance.UserInterface;

            _userInterface.Hud.SetActive(true);

            if (Debug.isDebugBuild)
            {
                _userInterface.DebuggingOverlay.SetActive(true);
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

            if (_userInterface != null && _userInterface.Hud != null)
            {
                _userInterface.Hud.SetActive(false);
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
        private void OnAttackHoldLeft()
        {
            OnAttackHold(true);
        }

        // ReSharper disable once UnusedMember.Local
        private void OnAttackHoldRight()
        {
            OnAttackHold(false);
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

            GameManager.Instance.UserInterface.DrawingPad.SetActive(true);
            GameManager.Instance.UserInterface.HudOverlay.ToggleCursorCapture(true);
        }

        // ReSharper disable once UnusedMember.Local
        private void OnShowCursorStop()
        {
            GameManager.Instance.UserInterface.DrawingPad.SetActive(false);
            GameManager.Instance.UserInterface.HudOverlay.ToggleCursorCapture(false);
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

            _inventoryDataService.PopulateInventoryChangesWithItem(invChange, craftedItem);

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
        public void CraftItemAsAdminServerRpc(string serialisedLoot, string categoryName, string craftableTypeName, bool isTwoHanded, string itemName)
        {
            GameManager.Instance.CheckIsAdmin();

            var loot = JsonUtility.FromJson<Loot>(serialisedLoot);
            loot.Id = Guid.NewGuid().ToMinimisedString();

            ((PlayerInventory)_playerState.Inventory).AddItemAsAdmin(loot);

            CraftItemServerRpc(loot.Id, categoryName, craftableTypeName, isTwoHanded, itemName);
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
        public void ShowHealthChangeClientRpc(Vector3 position, int change, ClientRpcParams clientRpcParams)
        {
            var offsetX = (float)ValueCalculator.Random.Next(-9, 10) / 100;
            var offsetY = (float)ValueCalculator.Random.Next(-9, 10) / 100;
            var offsetZ = (float)ValueCalculator.Random.Next(-9, 10) / 100;
            var adjustedPosition = position + new Vector3(offsetX, offsetY, offsetZ);

            var hit = Instantiate(_hitTextPrefab);
            hit.transform.SetParent(GameManager.Instance.UserInterface.HitNumberContainer.transform, false);

            var hitText = hit.GetComponent<TextMeshProUGUI>();

            hitText.text = change < 0
                ? (change * -1).ToString()
                : change.ToString();

            hitText.color = change <= 0
                ? Color.red
                : Color.green;

            const int maxDistanceForMinFontSize = 40;
            var distance = Vector3.Distance(Camera.main.transform.position, adjustedPosition);
            var fontSize = maxDistanceForMinFontSize - distance;
            if (fontSize < hitText.fontSizeMin) { fontSize = hitText.fontSizeMin; }
            else if (fontSize > hitText.fontSizeMax) { fontSize = hitText.fontSizeMax; }
            hitText.fontSize = fontSize;

            var sticky = hit.GetComponent<StickUiToWorldPosition>();
            sticky.WorldPosition = adjustedPosition;

            hit.SetActive(true);

            Destroy(hit, 1f);
        }

        #endregion

        private void UpdateMenuStates()
        {
            if (_toggleGameMenu || _toggleCharacterMenu)
            {
                if (_hasMenuOpen)
                {
                    _userInterface.HideAllMenus();
                }
                else if (_toggleGameMenu)
                {
                    _userInterface.HideOthersOpenThis(_userInterface.EscMenu);
                }
                else if (_toggleCharacterMenu)
                {
                    _userInterface.HideOthersOpenThis(_userInterface.CharacterMenu);
                }

                Tooltips.HideTooltip();

                _toggleGameMenu = false;
                _toggleCharacterMenu = false;
            }

            _hasMenuOpen = _userInterface.IsAnyMenuOpen();
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

        private void OnAttackHold(bool isLeftHand)
        {
            if (_hasMenuOpen || _playerState.AliveState != LivingEntityState.Alive)
            {
                return;
            }

            if (IsServer || _playerState.TryToAttackHold(isLeftHand))
            {
                _playerState.TryToAttackHoldServerRpc(isLeftHand);
            }
        }

        private void OnAttack(bool isLeftHand)
        {
            if (_hasMenuOpen || _playerState.AliveState != LivingEntityState.Alive)
            {
                return;
            }

            if (IsServer || _playerState.TryToAttack(isLeftHand))
            {
                _playerState.TryToAttackServerRpc(isLeftHand);
            }
        }

        private static void RefreshCraftingWindow()
        {
            var craftingUi = GameManager.Instance.UserInterface.GetCharacterMenuUiCraftingTab();

            if (craftingUi.gameObject.activeSelf)
            {
                craftingUi.ResetUi();
            }
        }

    }
}
