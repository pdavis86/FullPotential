using System;
using System.Linq;
using FullPotential.Api.Gameplay;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Gameplay.Crafting;
using FullPotential.Api.Gameplay.Inventory;
using FullPotential.Api.Ioc;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Localization;
using FullPotential.Api.Ui;
using FullPotential.Api.Unity.Constants;
using FullPotential.Api.Unity.Extensions;
using FullPotential.Api.Utilities.Extensions;
using FullPotential.Core.GameManagement;
using FullPotential.Core.GameManagement.Inventory;
using FullPotential.Core.Gameplay.Tooltips;
using FullPotential.Core.Networking;
using FullPotential.Core.Networking.Data;
using FullPotential.Core.UI.Behaviours;
using FullPotential.Core.UI.Events;
using FullPotential.Core.Utilities.UtilityBehaviours;
using TMPro;
using Unity.Netcode;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.Player
{
    public class PlayerBehaviour : NetworkBehaviour, IPlayerBehaviour
    {
        private const string EventSource = nameof(PlayerBehaviour);

        private readonly System.Random _random = new System.Random();

#pragma warning disable 0649
        [SerializeField] private Camera _playerCamera;
        [SerializeField] private Camera _inFrontOfPlayerCamera;
        [SerializeField] private GameObject _hitTextPrefab;
#pragma warning restore 0649

        //Services
        private IResultFactory _resultFactory;
        private IInventoryDataService _inventoryDataService;
        private ILocalizer _localizer;

        private bool _hasMenuOpen;
        private UserInterface _userInterface;
        private bool _toggleGameMenu;
        private bool _toggleCharacterMenu;
        private PlayerState _playerState;
        private PlayerMovement _playerMovement;
        private Interactable _focusedInteractable;
        private Camera _sceneCamera;
        private ClientRpcParams _clientRpcParams;
        private DrawingPadUi _drawingPadUi;

        private readonly FragmentedMessageReconstructor _inventoryChangesReconstructor = new FragmentedMessageReconstructor();

        #region Unity Event handlers

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            _playerState = GetComponent<PlayerState>();
            _playerMovement = GetComponent<PlayerMovement>();

            _resultFactory = DependenciesContext.Dependencies.GetService<IResultFactory>();
            _inventoryDataService = DependenciesContext.Dependencies.GetService<IInventoryDataService>();
            _localizer = DependenciesContext.Dependencies.GetService<ILocalizer>();

            _drawingPadUi = GameManager.Instance.UserInterface.DrawingPad.GetComponent<DrawingPadUi>();

            _drawingPadUi.OnDrawingStop += HandleOnDrawingStop;
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
            _playerState.InFrontOfPlayer.SetGameLayerRecursive(LayerMask.NameToLayer(Layers.InFrontOfPlayer));

            _inFrontOfPlayerCamera.gameObject.SetActive(true);
            _playerCamera.gameObject.SetActive(true);

            if (IsClient)
            {
                Camera.main.fieldOfView = GameManager.Instance.GameSettings.FieldOfView;
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
        private void OnAttackDownLeft()
        {
            if (GameManager.Instance.UserInterface.DrawingPad.activeInHierarchy)
            {
                _drawingPadUi.InitialiseForEquip(EventSource, HandSlotIds.LeftHand);
                _drawingPadUi.StartDrawing();
            }
        }

        // ReSharper disable once UnusedMember.Local
        private void OnAttackHoldLeft()
        {
            OnAttackHold(true);
        }

        // ReSharper disable once UnusedMember.Local
        private void OnAttackReleaseLeft()
        {
            if (GameManager.Instance.UserInterface.DrawingPad.activeInHierarchy)
            {
                _drawingPadUi.StopDrawing(HandSlotIds.LeftHand);
            }

            OnAttack(true);
        }

        // ReSharper disable once UnusedMember.Local
        private void OnAttackDownRight()
        {
            if (GameManager.Instance.UserInterface.DrawingPad.activeInHierarchy)
            {
                _drawingPadUi.InitialiseForEquip(EventSource, HandSlotIds.RightHand);
                _drawingPadUi.StartDrawing();
            }
        }

        // ReSharper disable once UnusedMember.Local
        private void OnAttackHoldRight()
        {
            OnAttackHold(false);
        }

        // ReSharper disable once UnusedMember.Local
        private void OnAttackReleaseRight()
        {
            if (GameManager.Instance.UserInterface.DrawingPad.activeInHierarchy)
            {
                _drawingPadUi.StopDrawing(HandSlotIds.RightHand);
            }

            OnAttack(false);
        }

        // ReSharper disable once UnusedMember.Local
        private void OnShowCursorStart()
        {
            if (_hasMenuOpen)
            {
                return;
            }

            GameManager.Instance.UserInterface.HudOverlay.ToggleDrawingMode(true);
            GameManager.Instance.UserInterface.DrawingPad.SetActive(true);
        }

        // ReSharper disable once UnusedMember.Local
        private void OnShowCursorStop()
        {
            GameManager.Instance.UserInterface.HudOverlay.ToggleDrawingMode(false);
            GameManager.Instance.UserInterface.DrawingPad.SetActive(false);
        }

        // ReSharper disable once UnusedMember.Local
        private void OnReloadLeft()
        {
            _playerState.TriggerReloadEvent(true);
        }

        // ReSharper disable once UnusedMember.Local
        private void OnReloadRight()
        {
            _playerState.TriggerReloadEvent(false);
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
        public void CraftItemServerRpc(string componentIdsCsv, string craftableTypeName, string typeId, bool isTwoHanded, string itemName)
        {
            var componentIdArray = componentIdsCsv.Split(',');

            var components = _playerState.PlayerInventory.GetComponentsFromIds(componentIdArray);

            if (components.Count != componentIdArray.Length)
            {
                Debug.LogWarning("Someone tried cheating: One or more IDs provided are not in the inventory");
                return;
            }

            var craftableType = (CraftableType)Enum.Parse(typeof(CraftableType), craftableTypeName);

            var craftedItem = _resultFactory.GetCraftedItem(
                craftableType,
                typeId,
                isTwoHanded,
                components
            );

            if (_playerState.PlayerInventory.ValidateIsCraftable(componentIdArray, craftedItem).Any())
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
        public void CraftItemAsAdminServerRpc(string serialisedLoot, string craftableTypeName, string typeId, bool isTwoHanded, string itemName)
        {
            GameManager.Instance.CheckIsAdmin();

            var loot = JsonUtility.FromJson<Loot>(serialisedLoot);
            loot.Id = Guid.NewGuid().ToMinimisedString();

            ((PlayerInventory)_playerState.Inventory).AddItemAsAdmin(loot);

            CraftItemServerRpc(loot.Id, craftableTypeName, typeId, isTwoHanded, itemName);
        }

        [ServerRpc]
        public void ClaimLootServerRpc(string id)
        {
            var skipIdCheck = Debug.isDebugBuild && id == "justgimmieloot";

            if (!skipIdCheck && !_playerState.ClaimLoot(id))
            {
                return;
            }

            InventoryChanges invChange;
            if (_random.Next(1, 3) == 1)
            {
                invChange = new InventoryChanges
                {
                    ItemStacks = new[] { _resultFactory.GetAmmoDrop() as ItemStack }
                };
            }
            else
            {
                invChange = new InventoryChanges
                {
                    Loot = new[] { _resultFactory.GetLootDrop() as Loot },
                };
            }

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
        public void ShowHealthChangeClientRpc(Vector3 position, int change, bool isCritical, ClientRpcParams clientRpcParams)
        {
            var offsetX = (float)_random.Next(-9, 10) / 100;
            var offsetY = (float)_random.Next(-9, 10) / 100;
            var offsetZ = (float)_random.Next(-9, 10) / 100;
            var adjustedPosition = position + new Vector3(offsetX, offsetY, offsetZ);

            var hit = Instantiate(_hitTextPrefab);
            hit.transform.SetParent(GameManager.Instance.UserInterface.HitNumberContainer.transform, false);

            var hitText = hit.GetComponent<TextMeshProUGUI>();

            hitText.text = isCritical
                ? _localizer.Translate("combat.attack.critical") + Math.Abs(change)
                : Math.Abs(change).ToString();

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
            if (_toggleGameMenu)
            {
                if (_hasMenuOpen)
                {
                    _userInterface.HideAllMenus();
                }
                else
                {
                    _userInterface.HideOthersOpenThis(_userInterface.EscMenu);
                }

                Tooltips.HideTooltip();

                _toggleGameMenu = false;
            }

            if (_toggleCharacterMenu)
            {
                if (_hasMenuOpen)
                {
                    if (_userInterface.CharacterMenu.activeInHierarchy)
                    {
                        _userInterface.HideAllMenus();
                    }
                }
                else
                {
                    _userInterface.HideOthersOpenThis(_userInterface.CharacterMenu);
                }

                Tooltips.HideTooltip();

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

        private void HandleOnDrawingStop(object sender, OnDrawingStopEventArgs e)
        {
            if (e.EventSource != EventSource)
            {
                return;
            }

            if (e.SlotId.IsNullOrWhiteSpace())
            {
                Debug.LogError("No slot was set so cannot equip any item");
                return;
            }

            var item = _playerState.PlayerInventory.GetItemFromAssignedShape(e.DrawnShape);

            if (item == null)
            {
                GameManager.Instance.GetUserInterface().HudOverlay.ShowAlert(_localizer.Translate("ui.drawingpad.nomatch"));
                return;
            }

            var playerInventory = (PlayerInventory)_playerState.Inventory;
            playerInventory.EquipItemServerRpc(item.Id, e.SlotId);
        }

    }
}
