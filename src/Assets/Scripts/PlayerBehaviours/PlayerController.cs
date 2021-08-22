using Assets.Core.Registry.Types;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using System;
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
// ReSharper disable RedundantDiscardDesignation

//todo: re-think this class. It should just be for the player to control their interation with the game

public class PlayerController : NetworkBehaviour
{
    public Camera PlayerCamera;
    public bool HasMenuOpen;

    private MainCanvasObjects _mainCanvasObjects;
    private bool _toggleGameMenu;
    private bool _toggleCharacterMenu;
    private PlayerInventory _inventory;
    private Interactable _focusedInteractable;
    private Hud _hud;

    #region Unity event handlers

    void Awake()
    {
        _mainCanvasObjects = GameManager.Instance.MainCanvasObjects;
        _mainCanvasObjects.CraftingUi.SetActive(false);

        _hud = _mainCanvasObjects.Hud.GetComponent<Hud>();

        if (Debug.isDebugBuild)
        {
            _mainCanvasObjects.DebuggingOverlay.SetActive(true);
        }

        _inventory = GetComponent<PlayerInventory>();
    }

    void OnInteract()
    {
        if (HasMenuOpen)
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
                if (HasMenuOpen)
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

            HasMenuOpen = _mainCanvasObjects.IsAnyMenuOpen();
            _mainCanvasObjects.Hud.SetActive(!HasMenuOpen);

            if (HasMenuOpen)
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

        if (_mainCanvasObjects.Hud != null)
        {
            _mainCanvasObjects.Hud.SetActive(false);
            _mainCanvasObjects.CraftingUi.SetActive(false);
        }
    }

    #endregion

    private void CheckForInteractable()
    {
        var ray = PlayerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
        if (Physics.Raycast(ray, out var hit, maxDistance: 1000))
        {
            var interactable = hit.collider.GetComponent<Interactable>();
            if (interactable != null)
            {
                var distance = Vector3.Distance(PlayerCamera.transform.position, interactable.transform.position);
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

    public void ShowAlert(string alertText)
    {
        //todo: cache this
        var pars = new ClientRpcParams { Send = new ClientRpcSendParams() { TargetClientIds = new[] { OwnerClientId } } };

        ShowAlertClientRpc(alertText, pars);
    }

    // ReSharper disable once UnusedParameter.Global
    [ClientRpc]
    public void ShowAlertClientRpc(string alertText, ClientRpcParams clientRpcParams = default)
    {
        _hud.ShowAlert(alertText);
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

        Debug.Log($"Trying to interact with {interactable.name}");

        var distance = Vector3.Distance(PlayerCamera.transform.position, interactable.transform.position);
        if (distance <= interactable.Radius)
        {
            interactable.OnInteract(serverRpcParams.Receive.SenderClientId);
        }
    }

    void TryToAttack(bool leftHand)
    {
        if (HasMenuOpen)
        {
            return;
        }

        CastSpellServerRpc(leftHand, PlayerCamera.transform.forward);
    }

    [ServerRpc]
    private void CastSpellServerRpc(bool leftHand, Vector3 direction, ServerRpcParams serverRpcParams = default)
    {
        var activeSpell = _inventory.GetSpellInHand(leftHand);

        if (activeSpell == null)
        {
            return;
        }

        switch (activeSpell.Targeting)
        {
            case Assets.Core.Spells.Targeting.Projectile _:
                SpawnSpellProjectile(activeSpell, leftHand, direction, serverRpcParams.Receive.SenderClientId);
                break;

            case Assets.Core.Spells.Targeting.Self _:
            case Assets.Core.Spells.Targeting.Touch _:
            case Assets.Core.Spells.Targeting.Beam _:
            case Assets.Core.Spells.Targeting.Cone _:
                //todo: other spell targeting options
                throw new NotImplementedException();

            default:
                throw new Exception($"Unexpected spell targeting with TypeName: '{activeSpell.Targeting.TypeName}'");
        }
    }

    private void SpawnSpellProjectile(Spell activeSpell, bool leftHand, Vector3 direction, ulong clientId)
    {
        if (!IsServer)
        {
            Debug.LogWarning("Tried to spawn a projectile when not on the server");
        }

        //todo: style projectile based on activeSpell

        var startPos = PlayerCamera.transform.position + PlayerCamera.transform.forward + new Vector3(leftHand ? -0.15f : 0.15f, -0.1f, 0);
        var spellObject = Instantiate(GameManager.Instance.Prefabs.Combat.Spell, startPos, Quaternion.identity, GameManager.Instance.MainCanvasObjects.RuntimeObjectsContainer.transform);

        var spellScript = spellObject.GetComponent<SpellBehaviour>();
        spellScript.PlayerClientId = new NetworkVariable<ulong>(clientId);
        spellScript.SpellId = new NetworkVariable<string>(activeSpell.Id);
        spellScript.SpellDirection = new NetworkVariable<Vector3>(direction);

        spellObject.GetComponent<NetworkObject>().Spawn(null, true);
    }

    //todo: move this method
    // ReSharper disable once UnusedParameter.Global
    [ClientRpc]
    public void ShowDamageClientRpc(Vector3 position, string damage, ClientRpcParams clientRpcParams = default)
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

}
