using System;
using System.Linq;
using FullPotential.Api.Data;
using FullPotential.Api.Gameplay;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Gameplay.Crafting;
using FullPotential.Api.Ioc;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Localization;
using FullPotential.Api.Ui;
using FullPotential.Api.Unity.Constants;
using FullPotential.Api.Unity.Extensions;
using FullPotential.Api.Utilities.Extensions;
using FullPotential.Core.GameManagement;
using FullPotential.Core.Gameplay.Tooltips;
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
        private ILocalizer _localizer;

        private bool _hasMenuOpen;
        private UserInterface _userInterface;
        private bool _toggleGameMenu;
        private bool _toggleCharacterMenu;
        private PlayerFighter _playerFighter;
        private PlayerMovement _playerMovement;
        private Interactable _focusedInteractable;
        private Camera _sceneCamera;
        private DrawingPadUi _drawingPadUi;


        #region Unity Event handlers

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            _playerFighter = GetComponent<PlayerFighter>();
            _playerMovement = GetComponent<PlayerMovement>();

            _resultFactory = DependenciesContext.Dependencies.GetService<IResultFactory>();
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

            _sceneCamera = Camera.main;
            if (_sceneCamera != null)
            {
                _sceneCamera.gameObject.SetActive(false);
            }

            //Avoids weapons clipping with other objects
            _playerFighter.InFrontOfPlayer.transform.parent = _inFrontOfPlayerCamera.transform;
            _playerFighter.InFrontOfPlayer.SetGameLayerRecursive(LayerMask.NameToLayer(Layers.InFrontOfPlayer));

            _inFrontOfPlayerCamera.gameObject.SetActive(true);
            _playerCamera.gameObject.SetActive(true);

            SetupLocalClient();
        }

        // ReSharper disable once UnusedMember.Local
        private void Update()
        {
            CheckForInteractable();
        }

        // ReSharper disable once UnusedMember.Local
        private void FixedUpdate()
        {
            //todo: zzz v0.6 stop using Update
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

        // ReSharper disable once UnusedMember.Local
        private void OnInteract()
        {
            if (IsNoUiInteractionPermitted())
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
            HandleAttackHold(true);
        }

        // ReSharper disable once UnusedMember.Local
        private void OnAttackReleaseLeft()
        {
            if (GameManager.Instance.UserInterface.DrawingPad.activeInHierarchy)
            {
                _drawingPadUi.StopDrawing(HandSlotIds.LeftHand);
            }
            else
            {
                HandleAttack(true);
            }
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
            HandleAttackHold(false);
        }

        // ReSharper disable once UnusedMember.Local
        private void OnAttackReleaseRight()
        {
            if (GameManager.Instance.UserInterface.DrawingPad.activeInHierarchy)
            {
                _drawingPadUi.StopDrawing(HandSlotIds.RightHand);
            }
            else
            {
                HandleAttack(false);
            }
        }

        // ReSharper disable once UnusedMember.Local
        private void OnShowCursorStart()
        {
            if (IsNoUiInteractionPermitted())
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
            _playerFighter.TriggerReloadFromClient(true);
        }

        // ReSharper disable once UnusedMember.Local
        private void OnReloadRight()
        {
            _playerFighter.TriggerReloadFromClient(false);
        }

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
        public void CraftItemServerRpc(string componentIdsCsv, string craftableTypeName, string typeId, string resourceTypeId, bool isTwoHanded, string itemName)
        {
            var componentIdArray = componentIdsCsv.Split(',');

            var components = _playerFighter.PlayerInventory.GetComponentsFromIds(componentIdArray);

            if (components.Count != componentIdArray.Length)
            {
                Debug.LogWarning("Someone tried cheating: One or more IDs provided are not in the inventory");
                return;
            }

            var craftableType = (CraftableType)Enum.Parse(typeof(CraftableType), craftableTypeName);

            var craftedItem = _resultFactory.GetCraftedItem(
                craftableType,
                typeId,
                resourceTypeId,
                isTwoHanded,
                components
            );

            if (_playerFighter.PlayerInventory.ValidateIsCraftable(componentIdArray, craftedItem).Any())
            {
                Debug.LogWarning("Someone tried cheating: validation was skipped");
                return;
            }

            if (!string.IsNullOrWhiteSpace(itemName))
            {
                craftedItem.Name = itemName;
            }

            var invChanges = new InventoryChanges
            {
                IdsToRemove = componentIdArray
            };

            _playerFighter.Inventory.PopulateInventoryChangesWithItem(invChanges, craftedItem);

            _playerFighter.Inventory.ApplyInventoryChanges(invChanges);
        }

        [ServerRpc]
        public void CraftItemAsAdminServerRpc(string serialisedLoot, string craftableTypeName, string typeId, string resourceTypeId, bool isTwoHanded, string itemName)
        {
            GameManager.Instance.CheckIsAdmin();

            var loot = JsonUtility.FromJson<Loot>(serialisedLoot);
            loot.Id = Guid.NewGuid().ToString();

            ((PlayerInventory)_playerFighter.Inventory).AddItemAsAdmin(loot);

            CraftItemServerRpc(loot.Id, craftableTypeName, typeId, resourceTypeId, isTwoHanded, itemName);
        }

        [ServerRpc]
        public void ClaimLootServerRpc(string id)
        {
            var skipIdCheck = Debug.isDebugBuild && id == "justgimmieloot";

            if (!skipIdCheck && !_playerFighter.ClaimLoot(id))
            {
                return;
            }

            InventoryChanges invChanges;
            if (_random.Next(1, 3) == 1)
            {
                invChanges = new InventoryChanges
                {
                    ItemStacks = new[] { _resultFactory.GetAmmoDrop() as ItemStack }
                };
            }
            else
            {
                invChanges = new InventoryChanges
                {
                    Loot = new[] { _resultFactory.GetLootDrop() as Loot },
                };
            }

            _playerFighter.Inventory.ApplyInventoryChanges(invChanges);
        }

        #endregion

        #region ClientRpc calls

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

        private void HandleAttackHold(bool isLeftHand)
        {
            if (IsNoUiInteractionPermitted())
            {
                return;
            }

            _playerFighter.TryToAttackHold(isLeftHand);
        }

        private void HandleAttack(bool isLeftHand)
        {
            if (IsNoUiInteractionPermitted())
            {
                return;
            }

            _playerFighter.TriggerAttackFromClient(isLeftHand);
        }

        private bool IsNoUiInteractionPermitted()
        {
            return _hasMenuOpen || _playerFighter.AliveState != LivingEntityState.Alive;
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

            var item = _playerFighter.PlayerInventory.GetItemFromAssignedShape(e.DrawnShape);

            if (item == null)
            {
                GameManager.Instance.GetUserInterface().HudOverlay.ShowAlert(_localizer.Translate("ui.drawingpad.nomatch"));
                return;
            }

            var playerInventory = (PlayerInventory)_playerFighter.Inventory;
            playerInventory.EquipItemServerRpc(item.Id, e.SlotId);
        }

        private void SetupLocalClient()
        {
            if (!IsClient)
            {
                return;
            }

            if (Debug.isDebugBuild)
            {
                _userInterface.DebuggingOverlay.SetActive(true);
            }

            Camera.main.fieldOfView = GameManager.Instance.GameSettings.FieldOfView;
        }
    }
}
