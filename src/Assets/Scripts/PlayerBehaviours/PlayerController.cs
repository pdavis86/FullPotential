using Assets.Scripts.Crafting.Results;
using Assets.Scripts.Data;
using System;
using System.Collections.Generic;
using System.Linq;
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

    private MainCanvasObjects _mainCanvasObjects;
    private bool _toggleEscMenu;
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

            if (Input.GetKeyDown(mappings.Menu)) { _toggleEscMenu = true; }
            else if (Input.GetKeyDown(mappings.Inventory)) { _toggleCharacterMenu = true; }
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
            if (_toggleEscMenu || _toggleCharacterMenu)
            {
                if (HasMenuOpen)
                {
                    _mainCanvasObjects.HideAllMenus();
                }
                else if (_toggleEscMenu)
                {
                    _mainCanvasObjects.HideOthersOpenThis(_mainCanvasObjects.EscMenu);
                }
                else if (_toggleCharacterMenu)
                {
                    _mainCanvasObjects.HideOthersOpenThis(_mainCanvasObjects.CharacterMenu);
                }

                HasMenuOpen = _mainCanvasObjects.IsAnyMenuOpen();
                _mainCanvasObjects.Hud.SetActive(!HasMenuOpen);

                _toggleEscMenu = false;
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

    [ServerCallback]
    private void SpawnSpellProjectile(Spell activeSpell, bool leftHand)
    {
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
    public void CmdInteractWith(NetworkInstanceId netId)
    {
        var go = NetworkServer.FindLocalObject(netId);
        var interactable = go.GetComponent<Interactable>();
        var distance = Vector3.Distance(PlayerCamera.transform.position, interactable.transform.position);
        if (distance <= interactable.Radius)
        {
            //Debug.Log("Interacted with " + interactable.gameObject.name);

            var lootDrop = GameManager.Instance.ResultFactory.GetLootDrop();

            //todo: Inventory.Add(lootDrop);

            var lootDropJson = JsonUtility.ToJson(new InventoryChange
            {
                Loot = new ItemBase[] { lootDrop }
            });
            connectionToClient.Send(Assets.Scripts.Networking.MessageIds.InventoryChange, new StringMessage(lootDropJson));
        }
    }

    [Command]
    public void CmdCraftItem(IEnumerable<string> componentIds, string selectedType, string selectedSubtype, bool isTwoHanded)
    {
        //Check that the components are actually in the player's inventory and load them in the order they are given
        var components = new List<ItemBase>();
        foreach (var id in componentIds)
        {
            components.Add(_inventory.Items.FirstOrDefault(x => x.Id == id));
        }

        if (components.Count != componentIds.Count())
        {
            Debug.LogError("One or more IDs provided are not in the inventory");
            return;
        }

        var craftedItem = GameManager.Instance.ResultFactory.GetCraftedItem(components, selectedType, selectedSubtype, isTwoHanded);

        var craftedType = craftedItem.GetType();

        var itemJson = JsonUtility.ToJson(new InventoryChange
        {
            IdsToRemove = componentIds.ToArray(),
            Accessories = craftedType == typeof(Accessory) ? new Accessory[] { craftedItem as Accessory } : null,
            Armor = craftedType == typeof(Armor) ? new Armor[] { craftedItem as Armor } : null,
            Spells = craftedType == typeof(Spell) ? new Spell[] { craftedItem as Spell } : null,
            Weapons = craftedType == typeof(Weapon) ? new Weapon[] { craftedItem as Weapon } : null
        });
        connectionToClient.Send(Assets.Scripts.Networking.MessageIds.InventoryChange, new StringMessage(itemJson));
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
