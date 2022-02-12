using System.Linq;
using FullPotential.Api;
using FullPotential.Api.Constants;
using FullPotential.Api.Data;
using FullPotential.Api.Enums;
using FullPotential.Api.Gameplay;
using FullPotential.Api.Helpers;
using FullPotential.Api.Registry.Gear;
using FullPotential.Api.Registry.Loot;
using FullPotential.Api.Registry.Spells;
using FullPotential.Core.Behaviours.GameManagement;
using FullPotential.Core.Behaviours.UtilityBehaviours;
using FullPotential.Core.Data;
using FullPotential.Core.Networking;
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
            GameObjectHelper.SetGameLayerRecursive(_playerState.InFrontOfPlayer, LayerMask.NameToLayer(Layers.InFrontOfPlayer));

            _inFrontOfPlayerCamera.gameObject.SetActive(true);
            _playerCamera.gameObject.SetActive(true);

            if (IsClient)
            {
                Camera.main.fieldOfView = ModHelper.GetGameManager().AppOptions.FieldOfView;
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
            _playerState.HandStatusLeft.IsReloading = true;
            _playerState.ReloadServerRpc(true);
        }

        // ReSharper disable once UnusedMember.Local
        private void OnReloadRight()
        {
            _playerState.HandStatusRight.IsReloading = true;
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
        private void TryToAttackServerRpc(bool isLeftHand)
        {
            var tellOtherClients = IsServer || TryToAttack(isLeftHand, _playerCamera.transform.position, _playerCamera.transform.forward, _playerState);
            if (tellOtherClients)
            {
                var nearbyClients = GameManager.Instance.RpcHelper.ForNearbyPlayersExcept(transform.position, OwnerClientId);
                _playerState.TryToAttackClientRpc(isLeftHand, _playerCamera.transform.position, _playerCamera.transform.forward, nearbyClients);
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

        public bool TryToAttack(bool isLeftHand, Vector3 position, Vector3 forwardDirection, IPlayerStateBehaviour playerStateBehaviour)
        {
            StopReloading(isLeftHand);

            var itemInHand = isLeftHand
                ? _playerState.Inventory.GetItemInSlot(SlotGameObjectName.LeftHand)
                : _playerState.Inventory.GetItemInSlot(SlotGameObjectName.RightHand);

            switch (itemInHand)
            {
                case null:
                    return Punch(position, forwardDirection) != null;

                case Spell spellInHand:
                    return CastSpell(spellInHand, isLeftHand, position, forwardDirection, playerStateBehaviour);

                case Weapon weaponInHand:
                    return UseWeapon(weaponInHand, isLeftHand, position, forwardDirection) != null;

                default:
                    Debug.LogWarning("Not implemented attack for " + itemInHand.Name + " yet");
                    return false;
            }
        }

        private ulong? Punch(Vector3 position, Vector3 forward)
        {
            if (!Physics.Raycast(position, forward, out var hit, 4))
            {
                return null;
            }

            if (IsServer)
            {
                GameManager.Instance.AttackHelper.DealDamage(gameObject, null, hit.transform.gameObject, hit.point);
            }

            return hit.transform.gameObject.GetComponent<NetworkObject>().NetworkObjectId;
        }

        public bool StopIfCastingSpell(PlayerHandStatus leftOrRight)
        {
            var behaviourToStop = leftOrRight.SpellBeingCastGameObject != null
                ? leftOrRight.SpellBeingCastGameObject.GetComponent<ISpellBehaviour>()
                : null;

            //Debug.Log("behaviourToStop is null: " + (behaviourToStop == null));

            if (behaviourToStop == null)
            {
                return false;
            }

            behaviourToStop.StopCasting();
            leftOrRight.SpellBeingCastGameObject = null;

            return true;
        }

        private bool CastSpell(Spell activeSpell, bool isLeftHand, Vector3 position, Vector3 forward, IPlayerStateBehaviour playerStateBehaviour)
        {
            if (activeSpell == null)
            {
                return false;
            }

            if (_playerState.Mana.Value < _playerState.GetManaCost(activeSpell))
            {
                return false;
            }

            var leftOrRight = isLeftHand
                ? _playerState.HandStatusLeft
                : _playerState.HandStatusRight;

            if (StopIfCastingSpell(leftOrRight))
            {
                //Return true as the action also needs performing on the server
                return true;
            }

            var startPosition = isLeftHand
                ? _playerState.Positions.LeftHandInFront.position
                : _playerState.Positions.RightHandInFront.position;

            const float maxDistance = 50f;
            var targetDirection = Physics.Raycast(position, forward, out var hit, maxDistance: maxDistance)
                ? (hit.point - startPosition).normalized
                : forward;

            var parentTransform = activeSpell.Targeting.IsParentedToCaster
                ? transform
                : GameManager.Instance.SceneBehaviour.GetTransform();

            GameManager.Instance.TypeRegistry.LoadAddessable(
                activeSpell.Targeting.PrefabAddress,
                prefab =>
                {
                    var spellObject = Instantiate(prefab, startPosition, Quaternion.identity);

                    activeSpell.Targeting.SetBehaviourVariables(spellObject, activeSpell, playerStateBehaviour, startPosition, targetDirection, isLeftHand);

                    spellObject.transform.parent = parentTransform;

                    leftOrRight.SpellBeingCast = activeSpell;

                    if (activeSpell.Targeting.IsContinuous)
                    {
                        leftOrRight.SpellBeingCastGameObject = spellObject;
                    }
                    
                    if (IsServer)
                    {
                        _playerState.SpendMana(activeSpell);
                    }
                }
            );

            if (activeSpell.Targeting.IsServerSideOnly && IsServer)
            {
                return false;
            }

            return true;
        }

        private ulong? UseWeapon(Weapon weaponInHand, bool isLeftHand, Vector3 position, Vector3 forward)
        {
            var registryType = (IGearWeapon)weaponInHand.RegistryType;

            return registryType.Category == IGearWeapon.WeaponCategory.Ranged
                ? UseRangedWeapon(weaponInHand, isLeftHand, position, forward)
                : UseMeleeWeapon(weaponInHand, position, forward);
        }

        private ulong? UseRangedWeapon(Weapon weaponInHand, bool isLeftHand, Vector3 position, Vector3 forward)
        {
            var ammoState = isLeftHand
                ? _playerState.HandStatusLeft
                : _playerState.HandStatusRight;

            if (ammoState.Ammo == 0)
            {
                return null;
            }

            //todo: attribute-based ammo consumption
            ammoState.Ammo -= 1;

            GameManager.Instance.MainCanvasObjects.HudOverlay.UpdateAmmo(isLeftHand, ammoState);

            var leftOrRight = isLeftHand
                ? _playerState.Positions.LeftHandInFront
                : _playerState.Positions.RightHandInFront;

            var startPos = leftOrRight.position + forward;

            //todo: attribute-based weapon range
            var endPos = Physics.Raycast(position, forward, out var rangedHit, 30)
                ? rangedHit.point
                : _playerState.Positions.RightHandInFront.position + forward * 30;

            var nearbyClients = GameManager.Instance.RpcHelper.ForNearbyPlayers(transform.position);
            _playerState.UsedWeaponClientRpc(startPos, endPos, nearbyClients);

            if (rangedHit.transform == null)
            {
                return null;
            }

            if (IsServer)
            {
                GameManager.Instance.AttackHelper.DealDamage(gameObject, weaponInHand, rangedHit.transform.gameObject, rangedHit.point);
            }

            return rangedHit.transform.gameObject.GetComponent<NetworkObject>()?.NetworkObjectId;
        }

        private ulong? UseMeleeWeapon(Weapon weaponInHand, Vector3 position, Vector3 forward)
        {
            //todo: attribute-based melee range
            if (!Physics.Raycast(position, forward, out var meleeHit, maxDistance: 4))
            {
                return null;
            }

            if (IsServer)
            {
                GameManager.Instance.AttackHelper.DealDamage(gameObject, weaponInHand, meleeHit.transform.gameObject, meleeHit.point);
            }

            return meleeHit.transform.gameObject.GetComponent<NetworkObject>().NetworkObjectId;
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

            if (!IsServer)
            {
                var ammoState = isLeftHand ? _playerState.HandStatusLeft : _playerState.HandStatusRight;
                if (ammoState.Ammo > 0)
                {
                    ammoState.Ammo -= 1;
                    GameManager.Instance.MainCanvasObjects.HudOverlay.UpdateAmmo(isLeftHand, ammoState);
                }
            }

            if (TryToAttack(isLeftHand, _playerCamera.transform.position, _playerCamera.transform.forward, _playerState))
            {
                TryToAttackServerRpc(isLeftHand);
            }
        }

        private void StopReloading(bool isLeftHand)
        {
            if (isLeftHand)
            {
                _playerState.HandStatusLeft.IsReloading = false;
            }
            else
            {
                _playerState.HandStatusRight.IsReloading = false;
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
