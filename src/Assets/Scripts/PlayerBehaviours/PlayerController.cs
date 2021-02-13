using Assets.Scripts.Crafting.Results;
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
    private SceneObjects001 _sceneObjects;


    private const short _myMsgType = 101;


    private Inventory _inventory;
    private Inventory Inventory
    {
        get
        {
            return _inventory ?? (_inventory = GetComponent<Inventory>());
        }
    }

    void Awake()
    {
        _sceneObjects = GameManager.GetSceneObjects().GetComponent<SceneObjects001>();
        _sceneObjects.UiCrafting.SetActive(false);
    }

    private void Start()
    {
        if (isClient)
        {
            connectionToServer.RegisterHandler(_myMsgType, OnServerSentMyMessageType);
        }
    }

    private void OnServerSentMyMessageType(NetworkMessage netMsg)
    {
        var beginMessage = netMsg.ReadMessage<StringMessage>();
        Debug.LogError("received message: " + beginMessage.value);
        //Debug.LogError("received message IsClient: " + isClient);
        //Debug.LogError("received message isServer: " + isServer);
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

                _sceneObjects.UiHud.SetActive(!_sceneObjects.UiHud.activeSelf);
                _sceneObjects.UiCrafting.SetActive(!_sceneObjects.UiHud.activeSelf);

                HasMenuOpen = !_sceneObjects.UiHud.activeSelf;
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

        if (_sceneObjects.UiHud != null)
        {
            _sceneObjects.UiHud.SetActive(false);
            _sceneObjects.UiCrafting.SetActive(false);
        }
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
        var spellObject = Instantiate(_sceneObjects.PrefabSpell, startPos, transform.rotation, transform);
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
            Debug.Log("Interacted with " + interactable.gameObject.name);



            //validation done... then what?

            var lootDrop = GameManager.Instance.ResultFactory.GetLootDrop();
            Inventory.Add(lootDrop);
            //Debug.Log($"Inventory now has {Inventory.Items.Count} items in it");

            //todo: send JSON to client
            connectionToClient.Send(_myMsgType, new StringMessage("This is a test message 1"));
            //NetworkServer.SendToClient(connectionToClient.connectionId, _myMsgType, new StringMessage("This is a test message 3"));
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
