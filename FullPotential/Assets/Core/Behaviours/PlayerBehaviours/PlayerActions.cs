using FullPotential.Core.Behaviours.GameManagement;
using FullPotential.Core.Data;
using FullPotential.Core.Helpers;
using FullPotential.Core.Networking;
using FullPotential.Core.Registry.Types;
using System;
using System.Linq;
using FullPotential.Api.Enums;
using FullPotential.Core.Behaviours.Ui;
using FullPotential.Core.Behaviours.UtilityBehaviours;
using FullPotential.Core.Combat;
using FullPotential.Core.Extensions;
using FullPotential.Standard.Spells.Targeting;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using Touch = FullPotential.Standard.Spells.Targeting.Touch;

// ReSharper disable UnusedMember.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Compiler
// ReSharper disable UnassignedField.Global
// ReSharper disable RedundantDiscardDesignation

namespace FullPotential.Core.Behaviours.PlayerBehaviours
{
    public class PlayerActions : NetworkBehaviour
    {
        [SerializeField] private Camera _playerCamera;
        [SerializeField] private Camera _inFrontOfPlayerCamera;
        [SerializeField] private GameObject _hitTextPrefab;

        private bool _hasMenuOpen;
        private MainCanvasObjects _mainCanvasObjects;
        private bool _toggleGameMenu;
        private bool _toggleCharacterMenu;
        private PlayerState _playerState;
        private PlayerMovement _playerMovement;
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
            GameObjectHelper.SetGameLayerRecursive(_playerState.InFrontOfPlayer, LayerMask.NameToLayer(Constants.Layers.InFrontOfPlayer));

            _inFrontOfPlayerCamera.gameObject.SetActive(true);
            _playerCamera.gameObject.SetActive(true);
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
        private void OnLeftAttack()
        {
            TryToAttack(true);
        }

        // ReSharper disable once UnusedMember.Local
        private void OnRightAttack()
        {
            TryToAttack(false);
        }

        // ReSharper disable once UnusedMember.Local
        private void OnShowCursorStart()
        {
            if (_hasMenuOpen)
            {
                return;
            }

            GameManager.Instance.MainCanvasObjects.DrawingPad.SetActive(true);
            GameManager.Instance.MainCanvasObjects.Hud.GetComponent<Hud>().ToggleCursorCapture(true);
        }

        // ReSharper disable once UnusedMember.Local
        private void OnShowCursorStop()
        {
            GameManager.Instance.MainCanvasObjects.DrawingPad.SetActive(false);
            GameManager.Instance.MainCanvasObjects.Hud.GetComponent<Hud>().ToggleCursorCapture(false);
        }

#pragma warning restore IDE0051 // Remove unused private members
        #endregion

        #region ServerRpc calls

        [ServerRpc]
        public void UpdatePlayerSettingsServerRpc(PlayerSettings playerSettings)
        {
            //Debug.Log("UpdatePlayerSettingsServerRpc called with " + playerSettings?.TextureUrl);

            var saveData = GameManager.Instance.UserRegistry.PlayerData[_playerState.Username];
            saveData.Settings = playerSettings ?? new PlayerSettings();
            saveData.IsDirty = true;

            _playerState.TextureUrl.Value = saveData.Settings.TextureUrl;
        }

        [ServerRpc]
        public void TryToAttackServerRpc(string itemId, Vector3 lookDirection, ServerRpcParams serverRpcParams = default)
        {
            if (itemId.IsNullOrWhiteSpace())
            {
                Punch();
                return;
            }

            var slotWithItem = _playerState.Inventory.GetEquippedWithItemId(itemId);
            var itemInHand = slotWithItem?.Value?.Item;

            if (!slotWithItem.HasValue
                || (slotWithItem.Value.Key != PlayerInventory.SlotGameObjectName.LeftHand && slotWithItem.Value.Key != PlayerInventory.SlotGameObjectName.RightHand)
                || itemInHand == null)
            {
                Debug.LogWarning("Player tried to cheat by sending an non-equipped item ID");
                return;
            }

            var isLeftHand = slotWithItem.Value.Key == PlayerInventory.SlotGameObjectName.LeftHand;

            if (itemInHand is Spell spellInHand)
            {
                CastSpell(spellInHand, isLeftHand, lookDirection, serverRpcParams);
            }
            else if (itemInHand is Weapon weaponInHand)
            {
                UseWeapon(weaponInHand);
            }
            else
            {
                Debug.LogWarning("Not implemented attack for " + itemInHand.Name + " yet");
            }
        }

        private void Punch()
        {
            var ray = _playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
            if (!Physics.Raycast(ray, out var hit, maxDistance: 4))
            {
                //Debug.Log("Swing and a miss!");
                return;
            }

            AttackHelper.DealDamage(gameObject, null, hit.transform.gameObject, hit.point);
        }

        private void UseWeapon(Weapon itemInHand)
        {
            var ray = _playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
            if (!Physics.Raycast(ray, out var hit, maxDistance: 4))
            {
                //Debug.Log("Weapon can't reach target!");
                return;
            }

            AttackHelper.DealDamage(gameObject, itemInHand, hit.transform.gameObject, hit.point);
        }

        private void CastSpell(Spell activeSpell, bool isLeftHand, Vector3 lookDirection, ServerRpcParams serverRpcParams)
        {
            if (!IsServer)
            {
                return;
            }

            var startPosition = isLeftHand
                ? _playerState.Positions.LeftHandInFront.position
                : _playerState.Positions.RightHandInFront.position;

            const float maxDistance = 50f;
            var targetDirection = Physics.Raycast(_playerCamera.transform.position, lookDirection, out var hit, maxDistance: maxDistance)
                ? (hit.point - startPosition).normalized
                : lookDirection;

            switch (activeSpell.Targeting)
            {
                case Projectile _:
                    _playerState.SpawnSpellProjectile(activeSpell, startPosition, targetDirection, serverRpcParams.Receive.SenderClientId);
                    break;

                case Self _:
                    _playerState.SpawnSpellSelf(activeSpell, startPosition, targetDirection, serverRpcParams.Receive.SenderClientId);
                    break;

                case Touch _:
                    _playerState.CastSpellTouch(activeSpell, startPosition, targetDirection, serverRpcParams.Receive.SenderClientId);
                    break;

                case Beam _:
                    _playerState.ToggleSpellBeam(isLeftHand, activeSpell, startPosition, targetDirection);
                    break;

                default:
                    throw new Exception($"Unexpected spell targeting with TypeName: '{activeSpell.Targeting.TypeName}'");
            }
        }

        [ServerRpc]
        public void TryToInteractServerRpc(string gameObjectName, ServerRpcParams serverRpcParams = default)
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

            //Debug.Log($"Trying to interact with {interactable.name}");

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

            var craftedItem = GameManager.Instance.ResultFactory.GetCraftedItem(
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

            _playerState.Inventory.PopulateInventoryChangesWithItem(invChange, craftedItem);

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

            var loot = GameManager.Instance.ResultFactory.GetLootDrop();
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

        // ReSharper disable once UnusedParameter.Global
        [ClientRpc]
        public void ApplyInventoryChangesClientRpc(string fragmentedMessageJson, ClientRpcParams clientRpcParams)
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
            _playerState.Inventory.ApplyInventoryChanges(changes);

            if (changes.IdsToRemove != null && changes.IdsToRemove.Length > 0)
            {
                RefreshCraftingWindow();
            }
        }

        private void CheckForInteractable()
        {
            var ray = _playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
            if (Physics.Raycast(ray, out var hit, maxDistance: 1000))
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

        private void TryToAttack(bool isLeftHand)
        {
            if (_hasMenuOpen || _playerState.AliveState != LivingEntityState.Alive)
            {
                return;
            }

            var itemInHand = _playerState.Inventory.GetItemInHand(isLeftHand);

            TryToAttackServerRpc(itemInHand?.Id, _playerCamera.transform.forward);
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

        public static void RefreshCraftingWindow()
        {
            var characterMenuUi = GameManager.Instance.MainCanvasObjects.CharacterMenu.GetComponent<CharacterMenuUi>();
            var craftingUi = characterMenuUi.Crafting.GetComponent<CharacterMenuUiCraftingTab>();

            if (craftingUi.gameObject.activeSelf)
            {
                craftingUi.ResetUi();
            }
        }

    }
}
