using System;
using System.Collections.Generic;
using System.Linq;

using Cysharp.Threading.Tasks;

using FullPotential.Api.Data;
using FullPotential.Api.Gameplay;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Gameplay.Crafting;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Gameplay.Player.Models;
using FullPotential.Api.Input;
using FullPotential.Api.Ioc;
using FullPotential.Api.Items;
using FullPotential.Api.Localization;
using FullPotential.Api.Logging;
using FullPotential.Api.Networking;
using FullPotential.Api.Obsolete.Items;
using FullPotential.Api.Obsolete.Items.Types;
using FullPotential.Api.Ui;
using FullPotential.Api.Unity.Constants;
using FullPotential.Api.Unity.Extensions;
using FullPotential.Api.Utilities.Extensions;
using FullPotential.Core.GameManagement;
using FullPotential.Core.Gameplay.Tooltips;
using FullPotential.Core.UI.Behaviours;
using FullPotential.Core.UI.Events;
using FullPotential.Core.Utilities.UtilityBehaviours;
using FullPotential.Models.Player;

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

        private readonly Collider[] _collidersInRange = new Collider[10];

#pragma warning disable 0649
        [SerializeField] private Camera _playerCamera;
        [SerializeField] private Camera _inFrontOfPlayerCamera;
        [SerializeField] private GameObject _hitTextPrefab;
#pragma warning restore 0649

        //Services
        private IAuditor _logger;
        private IEventBus _eventBus;
        private IResultFactory _resultFactory;
        private ILocalizer _localizer;
        private IItemFactory _itemFactory;
        private IDataSaver _dataSaver;
        private IRpcService _rpcService;

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

            _logger = DependenciesContext.Dependencies.GetService<IAuditorFactory>().Create(this);
            _eventBus = DependenciesContext.Dependencies.GetService<IEventBus>();
            _resultFactory = DependenciesContext.Dependencies.GetService<IResultFactory>();
            _localizer = DependenciesContext.Dependencies.GetService<ILocalizer>();
            _itemFactory = DependenciesContext.Dependencies.GetService<IItemFactory>();
            _dataSaver = DependenciesContext.Dependencies.GetService<IDataSaver>();
            _rpcService = DependenciesContext.Dependencies.GetService<IRpcService>();

            _userInterface = GameManager.Instance.UserInterface;
            _drawingPadUi = _userInterface.DrawingPad.GetComponent<DrawingPadUi>();

            _drawingPadUi.OnDrawingStop += HandleOnDrawingStop;
        }

        // ReSharper disable once UnusedMember.Local
        private void Start()
        {
            if (!IsOwner)
            {
                return;
            }

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

            if (Debug.isDebugBuild)
            {
                _userInterface.DebuggingOverlay.SetActive(true);
            }

            var settingsRepository = DependenciesContext.Dependencies.GetService<ISettingsRepository>();
            var gameSettings = settingsRepository.Get();
            Camera.main.fieldOfView = gameSettings.FieldOfView;
        }

        // ReSharper disable once UnusedMember.Local
        private void FixedUpdate()
        {
            CheckForInteractable();
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
            HandleAttackDown(HandSlotIds.LeftHand);
        }

        // ReSharper disable once UnusedMember.Local
        private void OnAttackHoldLeft()
        {
            HandleAttackHold(HandSlotIds.LeftHand);
        }

        // ReSharper disable once UnusedMember.Local
        private void OnAttackReleaseLeft()
        {
            HandleAttackRelease(HandSlotIds.LeftHand);
        }

        // ReSharper disable once UnusedMember.Local
        private void OnAttackDownRight()
        {
            HandleAttackDown(HandSlotIds.RightHand);
        }

        // ReSharper disable once UnusedMember.Local
        private void OnAttackHoldRight()
        {
            HandleAttackHold(HandSlotIds.RightHand);
        }

        // ReSharper disable once UnusedMember.Local
        private void OnAttackReleaseRight()
        {
            HandleAttackRelease(HandSlotIds.RightHand);
        }

        // ReSharper disable once UnusedMember.Local
        private void OnReloadLeft()
        {
            HandleReload(HandSlotIds.LeftHand);
        }

        // ReSharper disable once UnusedMember.Local
        private void OnReloadRight()
        {
            HandleReload(HandSlotIds.RightHand);
        }

        // ReSharper disable once UnusedMember.Local
        private void OnInteract()
        {
            if (IsCharacterInputDisabled())
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
        private void OnShowCursorStart()
        {
            if (IsCharacterInputDisabled())
            {
                return;
            }

            _userInterface.HudOverlay.ToggleDrawingMode(true);
            _userInterface.DrawingPad.SetActive(true);
        }

        // ReSharper disable once UnusedMember.Local
        private void OnShowCursorStop()
        {
            _userInterface.HudOverlay.ToggleDrawingMode(false);
            _userInterface.DrawingPad.SetActive(false);
        }

        #endregion

        #region Other ServerRpc calls

        [ServerRpc]
        private void TryToInteractServerRpc(string gameObjectName, ServerRpcParams serverRpcParams = default)
        {
            const float searchRadius = 5f;

            var playerNetworkObject = NetworkManager.Singleton.ConnectedClients[serverRpcParams.Receive.SenderClientId].PlayerObject;

            Interactable interactable = null;

            var colliderCount = Physics.OverlapSphereNonAlloc(playerNetworkObject.transform.position, searchRadius, _collidersInRange);
            for (var i = 0; i < colliderCount; i++)
            {
                if (_collidersInRange[i].gameObject.name == gameObjectName)
                {
                    if (!_collidersInRange[i].gameObject.TryGetComponent<Interactable>(out var colliderInteractable))
                    {
                        continue;
                    }

                    interactable = colliderInteractable;
                    break;
                }
            }

            if (interactable == null)
            {
                _logger.Error("Failed to find the interactable with gameObjectName " + gameObjectName);
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
                _logger.Warn("Someone tried cheating: One or more IDs provided are not in the inventory");
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
                _logger.Warn("Someone tried cheating: validation was skipped");
                return;
            }

            if (!string.IsNullOrWhiteSpace(itemName))
            {
                craftedItem.Name = itemName;
            }

            var items = componentIdArray
                .Select(x => new ItemData { Id = x, IsDeleted = true })
                .ToList();

            items.Add(_itemFactory.GetDataFromItem(_playerFighter.CharacterId, craftedItem));

            var invChanges = new InventoryData
            {
                Items = items
            };

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

            SaveLootAndUpdatePlayerAsync().Forget();
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

            var hit = Instantiate(
                _hitTextPrefab,
                _userInterface.HitNumberContainer.transform,
                false);

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

        private void CheckForInteractable()
        {
            var lookDirectionRay = _playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));

            if (Physics.Raycast(lookDirectionRay, out var hit, maxDistance: 1000))
            {
                if (hit.collider.TryGetComponent<Interactable>(out var interactable))
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

        private bool IsCharacterInputDisabled()
        {
            return _hasMenuOpen
                || _playerFighter.AliveState != LivingEntityState.Alive;
        }

        private void HandleOnDrawingStop(object sender, OnDrawingStopEventArgs e)
        {
            if (e.EventSource != EventSource)
            {
                return;
            }

            if (e.SlotId.IsNullOrWhiteSpace())
            {
                _logger.Error("No slot was set so cannot equip any item");
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

        private async UniTask SaveLootAndUpdatePlayerAsync()
        {
            var newItem = _random.Next(1, 3) == 1
                ? _resultFactory.GetAmmoDrop()
                : _resultFactory.GetLootDrop();

            var itemData = _itemFactory.GetDataFromItem(_playerFighter.CharacterId, newItem);

            var newItems = await _dataSaver.SaveInventoryAdditionsAndDeletionsAsync(_playerFighter.CharacterId, new List<ItemData> { itemData });
            var changes = new InventoryChangesForClient { IdsToFetch = new[] { newItems[0].Id } };

            var clientParams = _rpcService.ForPlayer(OwnerClientId);
            _playerFighter.Inventory.ApplyChangesClientRpc(changes, clientParams);
        }

        private void HandleAttackDown(string slotId)
        {
            _logger.Debug("OnAttackDown: " + slotId);

            if (_userInterface.DrawingPad.activeInHierarchy)
            {
                _drawingPadUi.InitialiseForEquip(EventSource, slotId);
                _drawingPadUi.StartDrawing();
            }
        }

        private void HandleAttackHold(string slotId)
        {
            if (IsCharacterInputDisabled())
            {
                return;
            }

            _eventBus.PublishAsync(new AttackHoldEventArgs(_playerFighter, slotId));

            if (!IsHost)
            {
                AttackHoldServerRpc(slotId);
            }
        }

        [ServerRpc]
        public void AttackHoldServerRpc(string slotId)
        {
            _eventBus.PublishAsync(new AttackHoldEventArgs(_playerFighter, slotId));
        }

        private void HandleAttackRelease(string slotId)
        {
            _logger.Debug("OnAttackRelease:" + slotId);

            if (_userInterface.DrawingPad.activeInHierarchy)
            {
                _drawingPadUi.StopDrawing();
                return;
            }

            if (IsCharacterInputDisabled())
            {
                return;
            }

            _eventBus.PublishAsync(new AttackReleaseEventArgs(_playerFighter, slotId));

            if (!IsHost)
            {
                AttackReleaseServerRpc(slotId);
            }
        }

        [ServerRpc]
        public void AttackReleaseServerRpc(string slotId)
        {
            _eventBus.PublishAsync(new AttackReleaseEventArgs(_playerFighter, slotId));
        }

        private void HandleReload(string slotId)
        {
            _eventBus.PublishAsync(new ReloadEventArgs(_playerFighter, slotId));

            if (!IsHost)
            {
                ReloadServerRpc(slotId);
            }
        }

        [ServerRpc]
        public void ReloadServerRpc(string slotId)
        {
            _eventBus.PublishAsync(new ReloadEventArgs(_playerFighter, slotId));
        }
    }
}
