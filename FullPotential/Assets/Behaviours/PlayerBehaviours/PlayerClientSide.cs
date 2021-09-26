using FullPotential.Assets.Core.Data;
using FullPotential.Assets.Core.Registry.Types;
using FullPotential.Assets.Core.Helpers;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.Serialization.Pooled;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using FullPotential.Assets.Core.Networking;

// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Compiler
// ReSharper disable UnassignedField.Global
// ReSharper disable RedundantDiscardDesignation

public class PlayerClientSide : NetworkBehaviour
{
#pragma warning disable 0649
    [SerializeField] private Camera _playerCamera;
    [SerializeField] private Camera _inFrontOfPlayerCamera;
    [SerializeField] private GameObject _hitTextPrefab;
#pragma warning restore 0649

    private bool _hasMenuOpen;
    private MainCanvasObjects _mainCanvasObjects;
    private bool _toggleGameMenu;
    private bool _toggleCharacterMenu;
    private PlayerState _playerState;
    private PlayerMovement _playerMovement;
    private Interactable _focusedInteractable;
    private Hud _hud;
    private Camera _sceneCamera;

    public PositionTransforms Positions;

    #region Unity event handlers

    private void Awake()
    {
        _playerState = GetComponent<PlayerState>();
        _playerMovement = GetComponent<PlayerMovement>();
    }

    private void Start()
    {
        //var localClientIdMatches = _playerState.ClientId.Value == NetworkManager.Singleton.LocalClientId;
        //Debug.LogError($"PlayerClientSide - IsOwner: {IsOwner}, localClientIdMatches: {localClientIdMatches}, IsLocalPlayer: {IsLocalPlayer}");

        if (!IsOwner)
        {
            return;
        }

        _mainCanvasObjects = GameManager.Instance.MainCanvasObjects;

        _hud = _mainCanvasObjects.Hud.GetComponent<Hud>();

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
        GameObjectHelper.SetGameLayerRecursive(_playerState.InFrontOfPlayer, FullPotential.Assets.Core.Constants.Layers.InFrontOfPlayer);

        _inFrontOfPlayerCamera.gameObject.SetActive(true);
        _playerCamera.gameObject.SetActive(true);

        CustomMessagingManager.RegisterNamedMessageHandler(nameof(MessageType.InventoryChange), OnInventoryChange);
        CustomMessagingManager.RegisterNamedMessageHandler(nameof(MessageType.LoadPlayerData), OnLoadPlayerData);
        CustomMessagingManager.RegisterNamedMessageHandler(nameof(MessageType.EquipChange), OnEquipChange);
        CustomMessagingManager.RegisterNamedMessageHandler(nameof(MessageType.EquipChanges), OnEquipChanges);

        RequestPlayerDataServerRpc();
        RequestingOtherPlayerDataServerRpc();
    }

    void OnInteract()
    {
        if (_hasMenuOpen)
        {
            return;
        }

        if (_focusedInteractable == null)
        {
            return;
        }

        InteractServerRpc(_focusedInteractable.gameObject.name);
    }

    void OnOpenCharacterMenu()
    {
        _toggleCharacterMenu = true;
    }

    void OnCancel()
    {
        _toggleGameMenu = true;
    }

    void OnLeftAttack()
    {
        TryToAttack(true);
    }

    void OnRightAttack()
    {
        TryToAttack(false);
    }

    void Update()
    {
        try
        {
            CheckForInteractable();

            //if (Input.GetKeyDown(mappings.GameMenu)) { _toggleGameMenu = true; }
            //else if (Input.GetKeyDown(mappings.CharacterMenu)) { _toggleCharacterMenu = true; }
            //else if (!HasMenuOpen)
            //{
            //    if (Input.GetKeyDown(mappings.Interact)) { TryToInteract(); }
            //    else if (Input.GetMouseButtonDown(0)) { TryToAttack(true); }
            //    else if (Input.GetMouseButtonDown(1)) { TryToAttack(false); }
            //    else
            //    {
            //        var mouseScrollWheel = Input.GetAxis("Mouse ScrollWheel");
            //        if (mouseScrollWheel > 0) { /*todo: scrolled up*/ Debug.Log("Positive mouse scroll"); }
            //        else if (mouseScrollWheel < 0) { /*todo: scrolled down*/ Debug.Log("Negative mouse scroll"); }
            //    }
            //}
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    private void FixedUpdate()
    {
        try
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
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    private void OnDisable()
    {
        if (!IsOwner)
        {
            return;
        }

        Cursor.lockState = CursorLockMode.None;

        if (_mainCanvasObjects?.Hud != null)
        {
            _mainCanvasObjects.Hud.SetActive(false);
        }

        if (_sceneCamera != null)
        {
            _sceneCamera.gameObject.SetActive(true);
        }
    }

    private void OnInventoryChange(ulong senderClientId, System.IO.Stream stream)
    {
        //Debug.LogError("Recieved OnInventoryChange network message");

        string message;
        using (PooledNetworkReader reader = PooledNetworkReader.Get(stream))
        {
            message = reader.ReadString().ToString();
        }

        var changes = JsonUtility.FromJson<InventoryAndRemovals>(message);
        HandleInventoryChange(changes);
    }

    private void OnLoadPlayerData(ulong senderClientId, System.IO.Stream stream)
    {
        //Debug.LogError("Recieved playerData from the server at clientId " + NetworkManager.Singleton.LocalClientId);

        string message;
        using (PooledNetworkReader reader = PooledNetworkReader.Get(stream))
        {
            message = reader.ReadString().ToString();
        }

        var playerData = JsonUtility.FromJson<PlayerData>(message);
        _playerState.LoadFromPlayerData(playerData);
    }

    private void OnEquipChange(ulong senderClientId, System.IO.Stream stream)
    {
        string message;
        using (PooledNetworkReader reader = PooledNetworkReader.Get(stream))
        {
            message = reader.ReadString().ToString();
        }

        var equipChange = JsonUtility.FromJson<EquipChange>(message);
        ApplyEquipChange(equipChange);
    }

    private void OnEquipChanges(ulong senderClientId, System.IO.Stream stream)
    {
        string message;
        using (PooledNetworkReader reader = PooledNetworkReader.Get(stream))
        {
            message = reader.ReadString().ToString();
        }

        var equipChanges = JsonUtility.FromJson<EquipChanges>(message);

        foreach (var equipChange in equipChanges.Changes)
        {
            ApplyEquipChange(equipChange);
        }
    }

    #endregion

    #region ServerRpc calls

    [ServerRpc]
    public void RequestPlayerDataServerRpc()
    {
        //Debug.LogError("Sending playerData to clientId " + OwnerClientId);

        var playerData = FullPotential.Assets.Core.Registry.UserRegistry.Load(null, _playerState.Username.Value);
        MessageHelper.SendMessageIfNotHost(playerData, nameof(MessageType.LoadPlayerData), OwnerClientId);
    }

    [ServerRpc]
    public void UpdatePlayerSettingsServerRpc(string textureUrl)
    {
        _playerState.TextureUrl.Value = textureUrl;
    }

    [ServerRpc]
    public void CastSpellServerRpc(bool isLeftHand, Vector3 startPosition, Vector3 direction, ServerRpcParams serverRpcParams = default)
    {
        var activeSpell = _playerState.Inventory.GetSpellInHand(isLeftHand);

        if (activeSpell == null)
        {
            return;
        }

        switch (activeSpell.Targeting)
        {
            case FullPotential.Assets.Core.Spells.Targeting.Projectile _:
                _playerState.SpawnSpellProjectile(activeSpell, startPosition, direction, serverRpcParams.Receive.SenderClientId);
                break;

            case FullPotential.Assets.Core.Spells.Targeting.Self _:
                _playerState.SpawnSpellSelf(activeSpell, startPosition, direction, serverRpcParams.Receive.SenderClientId);
                break;

            case FullPotential.Assets.Core.Spells.Targeting.Touch _:
                _playerState.CastSpellTouch(activeSpell, startPosition, direction, serverRpcParams.Receive.SenderClientId);
                break;

            case FullPotential.Assets.Core.Spells.Targeting.Beam _:
                _playerState.ToggleSpellBeam(isLeftHand, activeSpell, startPosition, serverRpcParams.Receive.SenderClientId);
                break;

            default:
                throw new Exception($"Unexpected spell targeting with TypeName: '{activeSpell.Targeting.TypeName}'");
        }
    }

    // ReSharper disable once UnusedParameter.Global
    [ServerRpc]
    public void InteractServerRpc(string gameObjectName, ServerRpcParams serverRpcParams = default)
    {
        const float searchRadius = 5f;

        var player = NetworkManager.Singleton.ConnectedClients[serverRpcParams.Receive.SenderClientId].PlayerObject;

        Interactable interactable = null;
        var collidersInRange = Physics.OverlapSphere(player.transform.position, searchRadius);
        foreach (var colliderNearby in collidersInRange)
        {
            if (colliderNearby.gameObject.name == gameObjectName)
            {
                var colliderInteractable = colliderNearby.gameObject.GetComponent<Interactable>();
                if (colliderInteractable != null)
                {
                    interactable = colliderInteractable;
                    break;
                }
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
            interactable.OnInteract(serverRpcParams.Receive.SenderClientId);
        }
    }

    [ServerRpc]
    public void CraftItemServerRpc(string[] componentIds, string categoryName, string craftableTypeName, bool isTwoHanded, string itemName)
    {
        var components = _playerState.Inventory.GetComponentsFromIds(componentIds);

        if (components.Count != componentIds.Length)
        {
            Debug.LogError("Someone tried cheating: One or more IDs provided are not in the inventory");
            return;
        }

        var craftedItem = GameManager.Instance.ResultFactory.GetCraftedItem(
            categoryName,
            craftableTypeName,
            isTwoHanded,
            components
        );

        if (_playerState.Inventory.ValidateIsCraftable(componentIds, craftedItem).Any())
        {
            Debug.LogError("Someone tried cheating: validation was skipped");
            return;
        }

        if (!string.IsNullOrWhiteSpace(itemName))
        {
            craftedItem.Name = itemName;
        }

        var craftedType = craftedItem.GetType();

        var invChange = new InventoryAndRemovals
        {
            IdsToRemove = componentIds.ToArray(),
            Accessories = craftedType == typeof(Accessory) ? new[] { craftedItem as Accessory } : null,
            Armor = craftedType == typeof(Armor) ? new[] { craftedItem as Armor } : null,
            Spells = craftedType == typeof(Spell) ? new[] { craftedItem as Spell } : null,
            Weapons = craftedType == typeof(Weapon) ? new[] { craftedItem as Weapon } : null
        };

        HandleInventoryChange(invChange);

        MessageHelper.SendMessageIfNotHost(invChange, nameof(MessageType.InventoryChange), OwnerClientId);
    }

    [ServerRpc]
    public void ChangeEquipsServerRpc(string[] equipSlots)
    {
        //Debug.LogError("Changing slots on server: " + IsServer);

        var invChange = new InventoryAndRemovals
        {
            EquipSlots = equipSlots
        };

        HandleInventoryChange(invChange);

        var equipChange = new EquipChange
        {
            SourceClientId = OwnerClientId,
            Inventory = _playerState.Inventory.GetDataForOtherPlayers()
        };

        //NOTE: No client ID. We want to tell all clients about equip changes
        MessageHelper.SendMessageIfNotHost(equipChange, nameof(MessageType.EquipChange));
    }

    [ServerRpc]
    public void RequestingOtherPlayerDataServerRpc()
    {
        var changes = new List<EquipChange>();

        var playerObjs = GameObject.FindGameObjectsWithTag(FullPotential.Assets.Core.Constants.Tags.Player);
        var playerStates = playerObjs.Select(x => x.GetComponent<PlayerState>());

        foreach (var otherPlayerState in playerStates)
        {
            if (otherPlayerState.OwnerClientId == OwnerClientId)
            {
                continue;
            }

            changes.Add(new EquipChange
            {
                SourceClientId = otherPlayerState.OwnerClientId,
                Inventory = otherPlayerState.Inventory.GetDataForOtherPlayers()
            });
        }

        var equipChanges = new EquipChanges
        {
            Changes = changes.ToArray()
        };

        MessageHelper.SendMessageIfNotHost(equipChanges, nameof(MessageType.EquipChanges), OwnerClientId);
    }

    #endregion

    private void ApplyEquipChange(EquipChange equipChange)
    {
        if (OwnerClientId == equipChange.SourceClientId)
        {
            return;
        }

        var otherPlayerState = PlayerState.GetWithClientId(equipChange.SourceClientId);
        otherPlayerState.Inventory.ApplyInventory(equipChange.Inventory);
    }

    private void HandleInventoryChange(InventoryAndRemovals changes)
    {
        _playerState.Inventory.ApplyInventoryAndRemovals(changes);

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
        if (_hasMenuOpen)
        {
            return;
        }

        var startPos = isLeftHand
            ? Positions.SpellStartLeft.position
            : Positions.SpellStartRight.position;

        CastSpellServerRpc(isLeftHand, startPos, _playerCamera.transform.forward);
    }

    public void ShowAlert(string alertText)
    {
        _hud.ShowAlert(alertText);
    }

    public void ShowDamage(Vector3 position, string damage)
    {
        var hit = Instantiate(_hitTextPrefab);
        hit.transform.SetParent(GameManager.Instance.MainCanvasObjects.HitNumberContainer.transform, false);
        hit.gameObject.SetActive(true);

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

    public void RefreshCraftingWindow()
    {
        var charaterMenuUi = GameManager.Instance.MainCanvasObjects.CharacterMenu.GetComponent<CharacterMenuUi>();
        var craftingUi = charaterMenuUi.Crafting.GetComponent<CharacterMenuUiCraftingTab>();

        if (craftingUi.gameObject.activeSelf)
        {
            craftingUi.ResetUi();
            craftingUi.LoadInventory();
        }
    }

    [System.Serializable]
    public class PositionTransforms
    {
        public Transform SpellStartLeft;
        public Transform SpellStartRight;
        public Transform LeftHand;
        public Transform RightHand;
    }

}
