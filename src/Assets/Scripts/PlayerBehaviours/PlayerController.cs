﻿using Assets.Scripts.Crafting.Results;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

// ReSharper disable once CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Compiler

public class PlayerController : NetworkBehaviour
{
    public Camera PlayerCamera;
    public bool HasMenuOpen;

    private bool _doUiToggle;

    void Awake()
    {
        GameManager.Instance.MainCanvasObjects.CraftingUi.SetActive(false);

        //todo: under what conditions?
        GameManager.Instance.MainCanvasObjects.DebuggingOverlay.SetActive(true);
    }

    void Update()
    {
        try
        {
            var mappings = GameManager.Instance.InputMappings;

            if (Input.GetKeyDown(mappings.Menu)) { _doUiToggle = true; }
            else if (Input.GetKeyDown(mappings.Inventory)) { OpenInventory(); }
            else if (Input.GetKeyDown(mappings.Interact)) { TryToInteract(); }
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

                GameManager.Instance.MainCanvasObjects.Hud.SetActive(!GameManager.Instance.MainCanvasObjects.Hud.activeSelf);
                GameManager.Instance.MainCanvasObjects.CraftingUi.SetActive(!GameManager.Instance.MainCanvasObjects.Hud.activeSelf);

                HasMenuOpen = !GameManager.Instance.MainCanvasObjects.Hud.activeSelf;
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

        if (GameManager.Instance.MainCanvasObjects.Hud != null)
        {
            GameManager.Instance.MainCanvasObjects.Hud.SetActive(false);
            GameManager.Instance.MainCanvasObjects.CraftingUi.SetActive(false);
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

    [ServerCallback]
    private void SpawnSpellProjectile(Spell activeSpell, bool leftHand)
    {
        var startPos = transform.position + PlayerCamera.transform.forward + new Vector3(leftHand ? -0.15f : 0.15f, -0.1f, 0);
        var spellObject = Instantiate(GameManager.Instance.Prefabs.Spell, startPos, transform.rotation, transform);
        spellObject.SetActive(true);

        var spellScript = spellObject.GetComponent<SpellBehaviour>();
        spellScript.PlayerNetworkId = netId.Value;

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
                    //interactable.InteractWith();
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
    public void CmdInteractWith(NetworkInstanceId netId)
    {
        var go = NetworkServer.FindLocalObject(netId);
        var interactable = go.GetComponent<Interactable>();
        var distance = Vector3.Distance(PlayerCamera.transform.position, interactable.transform.position);
        if (distance <= interactable.Radius)
        {
            //Debug.Log("Interacted with " + interactable.gameObject.name);

            var lootDrop = GameManager.Instance.ResultFactory.GetLootDrop();
            
            //todo: is this necessary too? - Inventory.Add(lootDrop);
            //Debug.Log($"Inventory now has {Inventory.Items.Count} items in it");

            var lootDropJson = JsonUtility.ToJson(lootDrop);
            connectionToClient.Send(Assets.Scripts.Networking.MessageIds.InventoryAddItem, new StringMessage(lootDropJson));
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
