using Assets.Core.Registry.Types;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Compiler
// ReSharper disable UnassignedField.Global

public class PlayerController : NetworkBehaviour
{
    public Camera PlayerCamera;
    public bool HasMenuOpen;

    private MainCanvasObjects _mainCanvasObjects;
    private bool _toggleGameMenu;
    private bool _toggleCharacterMenu;
    private PlayerInventory _inventory;

    private Interactable _focusedInteractable;


    #region Unity event handlers

    void Awake()
    {
        _mainCanvasObjects = GameManager.Instance.MainCanvasObjects;
        _mainCanvasObjects.CraftingUi.SetActive(false);

        if (Debug.isDebugBuild)
        {
            _mainCanvasObjects.DebuggingOverlay.SetActive(true);
        }

        _inventory = GetComponent<PlayerInventory>();
    }

    void Update()
    {
        try
        {
            CheckForInteractable();

            var mappings = GameManager.Instance.InputMappings;

            if (Input.GetKeyDown(mappings.GameMenu)) { _toggleGameMenu = true; }
            else if (Input.GetKeyDown(mappings.CharacterMenu)) { _toggleCharacterMenu = true; }
            else if (!HasMenuOpen)
            {
                if (Input.GetKeyDown(mappings.Interact)) { TryToInteract(); }
                else if (Input.GetMouseButtonDown(0)) { TryToAttack(true); }
                else if (Input.GetMouseButtonDown(1)) { TryToAttack(false); }
                else
                {
                    var mouseScrollWheel = Input.GetAxis("Mouse ScrollWheel");
                    if (mouseScrollWheel > 0) { /*todo: scrolled up*/ Debug.Log("Positive mouse scroll"); }
                    else if (mouseScrollWheel < 0) { /*todo: scrolled down*/ Debug.Log("Negative mouse scroll"); }
                }
            }
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

                HasMenuOpen = _mainCanvasObjects.IsAnyMenuOpen();
                _mainCanvasObjects.Hud.SetActive(!HasMenuOpen);

                Tooltips.HideTooltip();

                _toggleGameMenu = false;
                _toggleCharacterMenu = false;
            }

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

    void TryToInteract()
    {
        if (_focusedInteractable == null)
        {
            //todo: play a sound to indicate failed interaction
            return;
        }

        CmdInteractWith(_focusedInteractable.netId);
    }

    [Command]
    public void CmdInteractWith(NetworkInstanceId interactableNetId)
    {
        var interactable = NetworkServer.FindLocalObject(interactableNetId).GetComponent<Interactable>();

        Debug.Log($"Trying to interact with {interactable.name}");

        var distance = Vector3.Distance(PlayerCamera.transform.position, interactable.transform.position);
        if (distance <= interactable.Radius)
        {
            interactable.OnInteract(netId);
        }
    }

    void TryToAttack(bool leftHand)
    {
        //todo: implement this
        CmdCastSpell(leftHand);
    }

    [Command]
    private void CmdCastSpell(bool leftHand)
    {
        var activeSpell = _inventory.GetSpellInHand(leftHand);

        if (activeSpell == null)
        {
            return;
        }

        switch (activeSpell.Targeting)
        {
            case Assets.Core.Spells.Targeting.Projectile _:
                SpawnSpellProjectile(activeSpell, leftHand);
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

    [Server]
    private void SpawnSpellProjectile(Spell activeSpell, bool leftHand)
    {
        //todo: style projectile based on activeSpell

        var startPos = PlayerCamera.transform.position + PlayerCamera.transform.forward + new Vector3(leftHand ? -0.15f : 0.15f, -0.1f, 0);
        var spellObject = Instantiate(GameManager.Instance.Prefabs.Combat.Spell, startPos, transform.rotation, transform);
        spellObject.SetActive(true);

        var spellScript = spellObject.GetComponent<SpellBehaviour>();
        spellScript.PlayerNetworkId = netId.Value;
        spellScript.SpellId = activeSpell.Id;

        NetworkServer.Spawn(spellObject);
    }

    // ReSharper disable once UnusedParameter.Global
    [TargetRpc]
    public void TargetRpcShowDamage(NetworkConnection playerConnection, Vector3 position, string damage)
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
