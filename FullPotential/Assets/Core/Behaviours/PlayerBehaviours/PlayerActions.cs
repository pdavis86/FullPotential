using FullPotential.Core.Behaviours.GameManagement;
using FullPotential.Core.Data;
using FullPotential.Core.Helpers;
using FullPotential.Core.Networking;
using FullPotential.Core.Registry.Types;
using System;
using System.Linq;
using FullPotential.Api.Enums;
using FullPotential.Api.Registry;
using FullPotential.Core.Behaviours.UtilityBehaviours;
using FullPotential.Core.Combat;
using FullPotential.Standard.Spells.Targeting;
using TMPro;
using Unity.Netcode;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.Behaviours.PlayerBehaviours
{
    public class PlayerActions : NetworkBehaviour
    {
#pragma warning disable CS0649
        [SerializeField] private Camera _playerCamera;
        [SerializeField] private Camera _inFrontOfPlayerCamera;
        [SerializeField] private GameObject _hitTextPrefab;
#pragma warning restore CS0649

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
            OnAttack(true);
        }

        // ReSharper disable once UnusedMember.Local
        private void OnRightAttack()
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
            GameManager.Instance.MainCanvasObjects.GetHud().ToggleCursorCapture(true);
        }

        // ReSharper disable once UnusedMember.Local
        private void OnShowCursorStop()
        {
            GameManager.Instance.MainCanvasObjects.DrawingPad.SetActive(false);
            GameManager.Instance.MainCanvasObjects.GetHud().ToggleCursorCapture(false);
        }

        // ReSharper disable once UnusedMember.Local
        private void OnReloadLeft()
        {
            _playerState.AmmoStatusLeft.IsReloading = true;
            _playerState.ReloadServerRpc(true);
        }

        // ReSharper disable once UnusedMember.Local
        private void OnReloadRight()
        {
            _playerState.AmmoStatusRight.IsReloading = true;
            _playerState.ReloadServerRpc(false);
        }

#pragma warning restore IDE0051 // Remove unused private members
        #endregion

        #region ServerRpc calls

        [ServerRpc]
        public void UpdatePlayerSettingsServerRpc(PlayerSettings playerSettings)
        {
            var saveData = GameManager.Instance.UserRegistry.PlayerData[_playerState.Username];
            saveData.Settings = playerSettings ?? new PlayerSettings();
            saveData.IsDirty = true;

            _playerState.TextureUrl.Value = saveData.Settings.TextureUrl;
        }

        [ServerRpc]
        private void TryToAttackServerRpc(bool isLeftHand, ServerRpcParams serverRpcParams = default)
        {
            StopReloading(isLeftHand);

            var itemInHand = isLeftHand
                ? _playerState.Inventory.GetItemInSlot(PlayerInventory.SlotGameObjectName.LeftHand)
                : _playerState.Inventory.GetItemInSlot(PlayerInventory.SlotGameObjectName.RightHand);

            switch (itemInHand)
            {
                case null:
                    Punch();
                    break;

                case Spell spellInHand:
                    CastSpell(spellInHand, isLeftHand, serverRpcParams);
                    break;

                case Weapon weaponInHand:
                    UseWeapon(weaponInHand, isLeftHand);
                    break;

                default:
                    Debug.LogWarning("Not implemented attack for " + itemInHand.Name + " yet");
                    break;
            }
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

        #endregion

        private Ray GetLookDirectionRay()
        {
            return _playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
        }

        private void Punch()
        {
            if (!Physics.Raycast(GetLookDirectionRay(), out var hit, 4))
            {
                return;
            }

            AttackHelper.DealDamage(gameObject, null, hit.transform.gameObject, hit.point);
        }

        private void CastSpell(Spell activeSpell, bool isLeftHand, ServerRpcParams serverRpcParams)
        {
            if (!IsServer)
            {
                return;
            }

            var startPosition = isLeftHand
                ? _playerState.Positions.LeftHandInFront.position
                : _playerState.Positions.RightHandInFront.position;

            const float maxDistance = 50f;

            var lookDirection = GetLookDirectionRay().direction;

            var targetDirection = Physics.Raycast(_playerCamera.transform.position, lookDirection, out var hit, maxDistance: maxDistance)
                ? (hit.point - startPosition).normalized
                : lookDirection;

            //todo: generalise this so any spell from any mod will work
            switch (activeSpell.Targeting)
            {
                case Projectile:
                    _playerState.SpawnSpellProjectile(activeSpell, startPosition, targetDirection, serverRpcParams.Receive.SenderClientId);
                    break;

                case Self:
                    _playerState.SpawnSpellSelf(activeSpell, startPosition, targetDirection, serverRpcParams.Receive.SenderClientId);
                    break;

                case Standard.Spells.Targeting.Touch:
                    _playerState.CastSpellTouch(activeSpell, startPosition, targetDirection, serverRpcParams.Receive.SenderClientId);
                    break;

                case Beam:
                    _playerState.ToggleSpellBeam(isLeftHand, activeSpell, startPosition, targetDirection);
                    break;

                default:
                    throw new Exception($"Unexpected spell targeting with TypeName: '{activeSpell.Targeting.TypeName}'");
            }
        }

        private void UseWeapon(Weapon weaponInHand, bool isLeftHand)
        {
            var ray = _playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));

            var registryType = (IGearWeapon)weaponInHand.RegistryType;

            if (registryType.Category == IGearWeapon.WeaponCategory.Ranged)
            {
                var ammoState = isLeftHand
                    ? _playerState.AmmoStatusLeft
                    : _playerState.AmmoStatusRight;

                if (ammoState.Ammo == 0)
                {
                    return;
                }

                //todo: attribute-based ammo consumption
                ammoState.Ammo -= 1;

                GameManager.Instance.MainCanvasObjects.GetHud().UpdateAmmo(isLeftHand, ammoState);

                var lookDirection = GetLookDirectionRay().direction;

                var startPos = _playerState.Positions.RightHandInFront.position + lookDirection * 1;

                //todo: attribute-based weapon range
                var endPos = Physics.Raycast(ray, out var rangedHit, 30)
                      ? rangedHit.point
                      : _playerState.Positions.RightHandInFront.position + lookDirection * 30;

                _playerState.UsedWeaponClientRpc(startPos, endPos, RpcHelper.ForNearbyPlayers());

                if (rangedHit.transform != null)
                {
                    AttackHelper.DealDamage(gameObject, weaponInHand, rangedHit.transform.gameObject, rangedHit.point);
                }
            }
            else
            {
                //todo: attribute-based melee range
                if (!Physics.Raycast(ray, out var meleeHit, maxDistance: 4))
                {
                    return;
                }

                AttackHelper.DealDamage(gameObject, weaponInHand, meleeHit.transform.gameObject, meleeHit.point);
            }
        }

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

        private void OnAttack(bool isLeftHand)
        {
            if (_hasMenuOpen || _playerState.AliveState != LivingEntityState.Alive)
            {
                return;
            }

            if (!IsServer)
            {
                var ammoState = isLeftHand ? _playerState.AmmoStatusLeft : _playerState.AmmoStatusRight;
                if (ammoState.Ammo > 0)
                {
                    ammoState.Ammo -= 1;
                    GameManager.Instance.MainCanvasObjects.GetHud().UpdateAmmo(isLeftHand, ammoState);
                }
            }

            StopReloading(isLeftHand);

            TryToAttackServerRpc(isLeftHand);
        }

        private void StopReloading(bool isLeftHand)
        {
            if (isLeftHand)
            {
                _playerState.AmmoStatusLeft.IsReloading = false;
            }
            else
            {
                _playerState.AmmoStatusRight.IsReloading = false;
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

        private static void RefreshCraftingWindow()
        {
            var craftingUi = GameManager.Instance.MainCanvasObjects.GetCharacterMenuUiCraftingTab();

            if (craftingUi.gameObject.activeSelf)
            {
                craftingUi.ResetUi();
            }
        }

    }
}
