using Assets.Scripts.Crafting.Results;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// ReSharper disable once CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Compiler

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private Camera _playerCamera;

    public bool HasMenuOpen;

    private bool _doUiToggle;

    void Awake()
    {
        GameManager.Instance.GameObjects.UiCrafting.SetActive(false);
    }

    void Update()
    {
        try
        {
            var mappings = GameManager.Instance.InputMappings;

            if (Input.GetKeyDown(mappings.Menu)) { _doUiToggle = true; }
            else if (Input.GetKeyDown(mappings.Inventory)) { OpenInventory(); }
            else if (Input.GetKeyDown(mappings.Interact)) { InteractWith(); }
            else if (Input.GetMouseButtonDown(0)) { CmdCastSpell(false); }
            else if (Input.GetMouseButtonDown(1)) { CmdCastSpell(true); }
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
            if (_doUiToggle)
            {
                _doUiToggle = false;

                GameManager.Instance.GameObjects.UiHud.SetActive(!GameManager.Instance.GameObjects.UiHud.activeSelf);
                GameManager.Instance.GameObjects.UiCrafting.SetActive(!GameManager.Instance.GameObjects.UiHud.activeSelf);

                HasMenuOpen = !GameManager.Instance.GameObjects.UiHud.activeSelf;
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
        if (GameManager.Instance.GameObjects.UiHud != null)
        {
            GameManager.Instance.GameObjects.UiHud.SetActive(false);
            GameManager.Instance.GameObjects.UiCrafting.SetActive(false);
        }
    }






    //todo: move this
    private Spell GetPlayerActiveSpell()
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

    [Command]
    private void CmdCastSpell(bool leftHand)
    {
        if (HasMenuOpen)
        {
            return;
        }

        var activeSpell = GetPlayerActiveSpell();

        if (activeSpell == null)
        {
            return;
        }

        switch (activeSpell.Targeting)
        {
            case Spell.TargetingOptions.Projectile:
                var startPos = transform.position + _playerCamera.transform.forward + new Vector3(leftHand ? -0.15f : 0.15f, -0.1f, 0);
                var spellObject = Instantiate(GameManager.Instance.GameObjects.PrefabSpell, startPos, transform.rotation, transform);
                spellObject.SetActive(true);

                var castSpeed = activeSpell.Attributes.Speed / 50f;
                if (castSpeed < 0.5)
                {
                    castSpeed = 0.5f;
                }

                var spellRb = spellObject.GetComponent<Rigidbody>();
                spellRb.AddForce(_playerCamera.transform.forward * 20f * castSpeed, ForceMode.VelocityChange);

                var spellScript = spellObject.GetComponent<SpellBehaviour>();
                spellScript.SourcePlayer = GameManager.GetCurrentPlayerGameObject(_playerCamera);
                spellScript.Spell = activeSpell;

                NetworkServer.Spawn(spellObject);

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



    void InteractWith()
    {
        var startPos = _playerCamera.transform.position;
        if (Physics.Raycast(startPos, _playerCamera.transform.forward, out var hit))
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
                    interactable.InteractWith();
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

    void OpenInventory()
    {
        //todo: finish this
        Debug.Log("Tried to open inventory");
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

}
