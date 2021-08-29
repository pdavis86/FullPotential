using FullPotential.Assets.Core.Data;
using FullPotential.Assets.Core.Registry.Types;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.Serialization.Pooled;
using System;
using System.Linq;
using TMPro;
using UnityEngine;

// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Compiler
// ReSharper disable UnassignedField.Global

public class PlayerClientSide : NetworkBehaviour
{
    //todo: use compression for messages? - var jsonCompressed = Assets.Core.Helpers.CompressionHelper.CompressString(json);

#pragma warning disable 0649
    [SerializeField] private Camera _playerCamera;
    [SerializeField] private Camera _inFrontOfPlayerCamera;
    [SerializeField] private GameObject _spellStartLeft;
    [SerializeField] private GameObject _spellStartRight;
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
        _mainCanvasObjects.CraftingUi.SetActive(false);

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
        FullPotential.Assets.Helpers.GameObjectHelper.SetGameLayerRecursive(_playerState.InFrontOfPlayer, FullPotential.Assets.Constants.Layers.InFrontOfPlayer);

        _inFrontOfPlayerCamera.gameObject.SetActive(true);
        _playerCamera.gameObject.SetActive(true);

        CustomMessagingManager.RegisterNamedMessageHandler(nameof(FullPotential.Assets.Core.Networking.MessageType.InventoryChange), OnInventoryChange);
        CustomMessagingManager.RegisterNamedMessageHandler(nameof(FullPotential.Assets.Core.Networking.MessageType.LoadPlayerData), OnLoadPlayerData);

        RequestPlayerDataServerRpc();
    }

    void OnInteract()
    {
        if (_hasMenuOpen)
        {
            return;
        }

        if (_focusedInteractable == null)
        {
            //todo: play a sound to indicate a failed interaction
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
            //Can't see alerts if we diable the hud - _mainCanvasObjects.Hud.SetActive(!_hasMenuOpen);
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
        Cursor.lockState = CursorLockMode.None;

        if (_mainCanvasObjects?.Hud != null)
        {
            _mainCanvasObjects.Hud.SetActive(false);
            _mainCanvasObjects.CraftingUi.SetActive(false);
        }

        if (_sceneCamera != null)
        {
            _sceneCamera.gameObject.SetActive(true);
        }

        if (GameManager.Instance?.MainCanvasObjects?.Hud != null)
        {
            GameManager.Instance.MainCanvasObjects.Hud.SetActive(false);
        }
    }

    private void OnInventoryChange(ulong clientId, System.IO.Stream stream)
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

    public void HandleInventoryChange(InventoryAndRemovals changes)
    {
        _playerState.Inventory.ApplyInventoryAndRemovals(changes);

        _playerState.SpawnEquippedObjects();

        if (changes.IdsToRemove != null && changes.IdsToRemove.Length > 0)
        {
            RefreshCraftingWindow();
        }
    }

    private void OnLoadPlayerData(ulong clientId, System.IO.Stream stream)
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

    #endregion

    #region ServerRpc calls

    [ServerRpc]
    public void RequestPlayerDataServerRpc()
    {
        var playerData = FullPotential.Assets.Core.Registry.UserRegistry.LoadFromUsername(_playerState.Username.Value);

        //Debug.LogError("Sending playerData to clientId " + OwnerClientId);

        if (OwnerClientId > 0)
        {
            var json = JsonUtility.ToJson(playerData);
            var stream = PooledNetworkBuffer.Get();
            using (PooledNetworkWriter writer = PooledNetworkWriter.Get(stream))
            {
                writer.WriteString(json);
                CustomMessagingManager.SendNamedMessage(nameof(FullPotential.Assets.Core.Networking.MessageType.LoadPlayerData), OwnerClientId, stream);
            }
        }
    }

    [ServerRpc]
    public void UpdatePlayerSettingsServerRpc(string textureUrl)
    {
        _playerState.TextureUrl.Value = textureUrl;
    }

    [ServerRpc]
    public void CastSpellServerRpc(bool leftHand, ServerRpcParams serverRpcParams = default)
    {
        var activeSpell = _playerState.Inventory.GetSpellInHand(leftHand);

        if (activeSpell == null)
        {
            return;
        }

        var startPos = leftHand 
            ? _spellStartLeft.transform.position
            : _spellStartRight.transform.position;
        var direction = _playerCamera.transform.forward;

        switch (activeSpell.Targeting)
        {
            case FullPotential.Assets.Core.Spells.Targeting.Projectile _:
                _playerState.SpawnSpellProjectile(activeSpell, startPos, direction, serverRpcParams.Receive.SenderClientId);
                break;

            case FullPotential.Assets.Core.Spells.Targeting.Self _:
            case FullPotential.Assets.Core.Spells.Targeting.Touch _:
            case FullPotential.Assets.Core.Spells.Targeting.Beam _:
            case FullPotential.Assets.Core.Spells.Targeting.Cone _:
                //todo: other spell targeting options
                throw new NotImplementedException();

            default:
                throw new Exception($"Unexpected spell targeting with TypeName: '{activeSpell.Targeting.TypeName}'");
        }
    }

    // ReSharper disable once UnusedParameter.Global
    [ServerRpc]
    public void InteractServerRpc(string gameObjectName, ServerRpcParams serverRpcParams = default)
    {
        var player = NetworkManager.Singleton.ConnectedClients[serverRpcParams.Receive.SenderClientId].PlayerObject;

        Interactable interactable = null;
        //todo: replace hard-coded radius
        var collidersInRange = Physics.OverlapSphere(player.gameObject.transform.position, 5f);
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

        var distance = Vector3.Distance(gameObject.transform.position, interactable.transform.position);
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

        _playerState.Inventory.ApplyInventoryAndRemovals(invChange);

        if (OwnerClientId > 0)
        {
            var json = JsonUtility.ToJson(invChange);
            var stream = PooledNetworkBuffer.Get();
            using (PooledNetworkWriter writer = PooledNetworkWriter.Get(stream))
            {
                writer.WriteString(json);
                CustomMessagingManager.SendNamedMessage(nameof(FullPotential.Assets.Core.Networking.MessageType.InventoryChange), OwnerClientId, stream);
            }
        }
        else
        {
            HandleInventoryChange(invChange);
        }
    }

    [ServerRpc]
    public void ChangeEquipsServerRpc()
    {
        //Debug.LogError("Changing slots on server: " + IsServer);

        var invChange = new InventoryAndRemovals
        {
            EquipSlots = _playerState.Inventory.EquipSlots
        };

        //todo: make this IF(IsHost) a method and use in all SendNamedMessage places
        if (OwnerClientId > 0)
        {
            var json = JsonUtility.ToJson(invChange);
            var stream = PooledNetworkBuffer.Get();
            using (PooledNetworkWriter writer = PooledNetworkWriter.Get(stream))
            {
                writer.WriteString(json);
                CustomMessagingManager.SendNamedMessage(nameof(FullPotential.Assets.Core.Networking.MessageType.InventoryChange), OwnerClientId, stream);
            }
        }
        else
        {
            HandleInventoryChange(invChange);
        }
    }

    #endregion

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

    private void TryToAttack(bool leftHand)
    {
        if (_hasMenuOpen)
        {
            return;
        }

        CastSpellServerRpc(leftHand);
    }

    public void ShowAlert(string alertText)
    {
        _hud.ShowAlert(alertText);
    }

    public void ShowDamage(Vector3 position, string damage)
    {
        var hit = Instantiate(GameManager.Instance.Prefabs.Combat.HitText);
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
        var craftingUi = GameManager.Instance.MainCanvasObjects.CraftingUi.GetComponent<CraftingMenuUi>();
        if (craftingUi.gameObject.activeSelf)
        {
            craftingUi.ResetUi();
            craftingUi.LoadInventory();
        }
    }

}
