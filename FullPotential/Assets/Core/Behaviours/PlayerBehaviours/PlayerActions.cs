﻿using FullPotential.Assets.Core.Data;
using FullPotential.Assets.Core.Helpers;
using FullPotential.Assets.Core.Networking;
using FullPotential.Assets.Core.Registry.Types;
using System;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;

// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Compiler
// ReSharper disable UnassignedField.Global
// ReSharper disable RedundantDiscardDesignation

//todo: see if the problem is that I have too many network behaviours

public class PlayerActions : NetworkBehaviour
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
    private ClientRpcParams _clientRpcParams;
    
    private readonly FragmentedMessageReconstructor _inventoryChangesReconstructor = new FragmentedMessageReconstructor();

    #region Event handlers

    private void Awake()
    {
        _playerState = GetComponent<PlayerState>();
        _playerMovement = GetComponent<PlayerMovement>();
    }

    private void Start()
    {
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

        _clientRpcParams.Send.TargetClientIds = new[] { OwnerClientId };
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

        TryToInteractServerRpc(_focusedInteractable.gameObject.name);
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
        CheckForInteractable();
    }

    private void FixedUpdate()
    {
        UpdateMenuStates();
    }

    private void OnDisable()
    {
        if (!IsOwner)
        {
            return;
        }

        Cursor.lockState = CursorLockMode.None;

        if (_mainCanvasObjects.Hud != null)
        {
            _mainCanvasObjects.Hud.SetActive(false);
        }

        if (_sceneCamera != null)
        {
            _sceneCamera.gameObject.SetActive(true);
        }
    }

    #endregion

    #region ServerRpc calls

    [ServerRpc]
    public void UpdatePlayerSettingsServerRpc(string textureUrl)
    {
        _playerState.TextureUrl.Value = textureUrl;
    }

    [ServerRpc]
    public void TryToAttackServerRpc(string itemId, Vector3 direction, ServerRpcParams serverRpcParams = default)
    {
        var isLeftHand = false;
        if (_playerState.Inventory.EquippedLeftHand.Value == itemId)
        {
            isLeftHand = true;
        }
        else if (_playerState.Inventory.EquippedRightHand.Value != itemId)
        {
            Debug.LogError("Player tried to cheat by sending an un-equipped item ID");
            return;
        }

        var itemInHand = _playerState.Inventory.GetItemInHand(isLeftHand);

        if (itemInHand is Spell spellInHand)
        {
            CastSpell(spellInHand, isLeftHand, direction, serverRpcParams);
        }
        else
        {
            Debug.LogWarning("Not implemented attack for " + itemInHand.Name + " yet");
        }
    }

    private void CastSpell(Spell activeSpell, bool isLeftHand, Vector3 direction, ServerRpcParams serverRpcParams)
    {
        if (!IsServer)
        {
            return;
        }

        var startPosition = isLeftHand
            ? _playerState.Positions.LeftHandInFront.position
            : _playerState.Positions.RightHandInFront.position;

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
                _playerState.ToggleSpellBeam(isLeftHand, activeSpell, startPosition, direction, serverRpcParams.Receive.SenderClientId);
                break;

            default:
                throw new Exception($"Unexpected spell targeting with TypeName: '{activeSpell.Targeting.TypeName}'");
        }
    }

    // ReSharper disable once UnusedParameter.Global
    [ServerRpc]
    public void TryToInteractServerRpc(string gameObjectName, ServerRpcParams serverRpcParams = default)
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
    public void CraftItemServerRpc(string componentIdsCsv, string categoryName, string craftableTypeName, bool isTwoHanded, string itemName)
    {
        var componentIdArray = componentIdsCsv.Split(',');

        var components = _playerState.Inventory.GetComponentsFromIds(componentIdArray);

        if (components.Count != componentIdArray.Length)
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

        if (_playerState.Inventory.ValidateIsCraftable(componentIdArray, craftedItem).Any())
        {
            Debug.LogError("Someone tried cheating: validation was skipped");
            return;
        }

        if (!string.IsNullOrWhiteSpace(itemName))
        {
            craftedItem.Name = itemName;
        }

        var craftedType = craftedItem.GetType();

        var invChange = new InventoryChanges
        {
            IdsToRemove = componentIdArray,
            Accessories = craftedType == typeof(Accessory) ? new[] { craftedItem as Accessory } : null,
            Armor = craftedType == typeof(Armor) ? new[] { craftedItem as Armor } : null,
            Spells = craftedType == typeof(Spell) ? new[] { craftedItem as Spell } : null,
            Weapons = craftedType == typeof(Weapon) ? new[] { craftedItem as Weapon } : null
        };

        if (OwnerClientId != 0)
        {
            ApplyInventoryChanges(invChange);
        }

        foreach (var message in MessageHelper.GetFragmentedMessages(JsonUtility.ToJson(invChange)))
        {
            ApplyInventoryChangesClientRpc(JsonUtility.ToJson(message), _clientRpcParams);
        }
    }

    #endregion

    #region ClientRpc calls

    // ReSharper disable once UnusedParameter.Global
    [ClientRpc]
    public void ApplyInventoryChangesClientRpc(string fragmentedMessageJson, ClientRpcParams clientRpcParams = default)
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
        if (_hasMenuOpen)
        {
            return;
        }

        var itemInHand = _playerState.Inventory.GetItemInHand(isLeftHand);
        TryToAttackServerRpc(itemInHand.Id, _playerCamera.transform.forward);
    }

    public void ShowAlert(string alertText)
    {
        _hud.ShowAlert(alertText);
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

    public void RefreshCraftingWindow()
    {
        var charaterMenuUi = GameManager.Instance.MainCanvasObjects.CharacterMenu.GetComponent<CharacterMenuUi>();
        var craftingUi = charaterMenuUi.Crafting.GetComponent<CharacterMenuUiCraftingTab>();

        if (craftingUi.gameObject.activeSelf)
        {
            craftingUi.ResetUi();
        }
    }

}