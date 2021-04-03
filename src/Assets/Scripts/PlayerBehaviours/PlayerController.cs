using Assets.Core.Crafting;
using System;
using System.Collections.Generic;
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
    private Inventory _inventory;

    void Awake()
    {
        _mainCanvasObjects = GameManager.Instance.MainCanvasObjects;
        _mainCanvasObjects.CraftingUi.SetActive(false);

        //todo: under what conditions?
        _mainCanvasObjects.DebuggingOverlay.SetActive(true);
    }

    private void Start()
    {
        _inventory = GameManager.Instance.LocalPlayer.GetComponent<Inventory>();
    }

    void Update()
    {
        try
        {
            var mappings = GameManager.Instance.InputMappings;

            if (Input.GetKeyDown(mappings.GameMenu)) { _toggleGameMenu = true; }
            else if (Input.GetKeyDown(mappings.CharacterMenu)) { _toggleCharacterMenu = true; }
            else if (!HasMenuOpen)
            {
                if (Input.GetKeyDown(mappings.Interact)) { TryToInteract(); }
                else if (Input.GetMouseButtonDown(0)) { CmdCastSpell(false); }
                else if (Input.GetMouseButtonDown(1)) { CmdCastSpell(true); }
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

    //private void OnDestroy()
    //{
    //    Debug.Log("OnDestroy() called");
    //}

    //private void OnServerInitialized()
    //{
    //    Debug.Log("OnServerInitialized() called");
    //}





    [Command]
    private void CmdCastSpell(bool leftHand)
    {
        var activeSpell = GetPlayerActiveSpell();

        if (activeSpell == null)
        {
            return;
        }

        switch (activeSpell.Targeting)
        {
            case Spell.TargetingOptions.Projectile:
                SpawnSpellProjectile(activeSpell, leftHand);
                break;

            //todo: other spell targeting options
            //case Spell.TargetingOptions.Self:
            //case Spell.TargetingOptions.Touch:
            //case Spell.TargetingOptions.Beam:
            //case Spell.TargetingOptions.Cone:

            default:
                throw new Exception("Unexpected spell targeting: " + activeSpell.Targeting);
        }
    }

    [Server]
    private void SpawnSpellProjectile(Spell activeSpell, bool leftHand)
    {
        //todo: style projectile based on activeSpell

        var startPos = PlayerCamera.transform.position + PlayerCamera.transform.forward + new Vector3(leftHand ? -0.15f : 0.15f, -0.1f, 0);
        var spellObject = Instantiate(GameManager.Instance.Prefabs.Spell, startPos, transform.rotation, transform);
        spellObject.SetActive(true);

        var spellScript = spellObject.GetComponent<SpellBehaviour>();
        spellScript.PlayerNetworkId = netId.Value;

        //todo: why is there a second projectile in the sky?

        NetworkServer.Spawn(spellObject);
    }

    void TryToInteract()
    {
        var startPos = PlayerCamera.transform.position;
        if (Physics.Raycast(startPos, PlayerCamera.transform.forward, out var hit))
        {
            //Debug.DrawLine(startPos, hit.point, Color.blue, 3);
            //Debug.Log("Ray cast hit " + hit.collider.gameObject.name);

            var interactable = hit.collider.GetComponent<Interactable>();
            if (interactable != null)
            {
                var distance = Vector3.Distance(startPos, interactable.transform.position);
                if (distance <= interactable.Radius)
                {
                    //Debug.Log("Interacted with " + hit.collider.gameObject.name);
                    CmdInteractWith(interactable.netId);
                }
                //else
                //{
                //    Debug.Log($"But not close enough ({distance})");
                //}
            }
            //else
            //{
            //    Debug.Log("But it's not interactable");
            //}
        }
    }

    [Command]
    public void CmdInteractWith(NetworkInstanceId instanceId)
    {
        var go = NetworkServer.FindLocalObject(instanceId);
        var interactable = go.GetComponent<Interactable>();
        var distance = Vector3.Distance(PlayerCamera.transform.position, interactable.transform.position);
        if (distance <= interactable.Radius)
        {
            //Debug.Log("Interacted with " + interactable.gameObject.name);

            //todo: move this into a script on the interactable
            _inventory.Add(GameManager.Instance.ResultFactory.GetLootDrop());
        }
    }




    //public static bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaysastResults)
    //{
    //    var uiLayer = LayerMask.NameToLayer("UI");
    //    return GetEventSystemRaycastResults().FirstOrDefault(x => x.gameObject.layer == uiLayer).gameObject != null;
    //}

    //static List<RaycastResult> GetEventSystemRaycastResults()
    //{
    //    var raycastResults = new List<RaycastResult>();
    //    EventSystem.current.RaycastAll(
    //        new PointerEventData(EventSystem.current)
    //        {
    //            position = Input.mousePosition
    //        }, raycastResults
    //    );
    //    return raycastResults;
    //}








    //todo: move this
    public Spell GetPlayerActiveSpell()
    {
        //todo: check the player has a spell active and can cast it
        return new Spell
        {
            Name = "test spell",
            Targeting = Spell.TargetingOptions.Projectile,
            Attributes = new Attributes
            {
                Strength = 50
            },
            Effects = new List<string> { Spell.ElementalEffects.Fire },
            Shape = Spell.ShapeOptions.Wall
        };
    }

}
