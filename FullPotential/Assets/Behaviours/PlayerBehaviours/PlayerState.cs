using FullPotential.Assets.Api.Registry;
using FullPotential.Assets.Core.Data;
using FullPotential.Assets.Core.Registry.Base;
using FullPotential.Assets.Core.Registry.Types;
using FullPotential.Assets.Core.Storage;
using FullPotential.Assets.Helpers;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using TMPro;
using UnityEngine;
using static FullPotential.Assets.Core.Storage.PlayerInventory;

// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Compiler
// ReSharper disable ConvertToUsingDeclaration
// ReSharper disable RedundantDiscardDesignation
// ReSharper disable UnassignedField.Global

public class PlayerState : NetworkBehaviour
{
#pragma warning disable 0649
    [SerializeField] private Behaviour[] _behavioursToDisable;
    [SerializeField] private TextMeshProUGUI _nameTag;
    [SerializeField] private MeshRenderer _mainMesh;
    [SerializeField] private MeshRenderer _leftMesh;
    [SerializeField] private MeshRenderer _rightMesh;
#pragma warning restore 0649

    public GameObject InFrontOfPlayer;

    public readonly NetworkVariable<ulong> ClientId = new NetworkVariable<ulong>(9999);
    public readonly NetworkVariable<string> Username = new NetworkVariable<string>();
    public readonly NetworkVariable<string> TextureUrl = new NetworkVariable<string>();

    public readonly PlayerInventory Inventory;

    private bool _loadWasSuccessful;
    private PlayerClientSide _playerClientSide;
    private ClientRpcParams _clientRpcParams;

    public PlayerState()
    {
        Inventory = new PlayerInventory(this);
    }

    #region Event handlers

    private void Awake()
    {
        Username.OnValueChanged += OnUsernameChanged;
        TextureUrl.OnValueChanged += OnTextureChanged;

        _playerClientSide = GetComponent<PlayerClientSide>();
    }

    private void Start()
    {
        //_localClientIdMatches = ClientId.Value == NetworkManager.Singleton.LocalClientId;
        //Debug.LogError($"PlayerState - IsOwner: {IsOwner}, localClientIdMatches: {_localClientIdMatches}, IsLocalPlayer: {IsLocalPlayer}");

        if (IsOwner)
        {
            GameManager.Instance.DataStore.LocalPlayer = gameObject;
        }
        else
        {
            foreach (var comp in _behavioursToDisable)
            {
                comp.enabled = false;
            }

            //todo: these should not be necessary calls for the client, right?
            //SetNameTag();
            //SetTexture();
        }

        gameObject.name = "Player ID " + NetworkObjectId;


        _clientRpcParams.Send.TargetClientIds = new[] { OwnerClientId };
        //_clientRpcParams = new ClientRpcParams { Send = new ClientRpcSendParams() { TargetClientIds = new[] { OwnerClientId } } };
    }

    private void OnDisable()
    {
        Username.OnValueChanged -= OnUsernameChanged;
        TextureUrl.OnValueChanged -= OnTextureChanged;
    }

    private void OnUsernameChanged(string previousValue, string newValue)
    {
        SetNameTag();
    }

    private void OnTextureChanged(string previousValue, string newValue)
    {
        SetTexture();
    }

    #endregion

    #region ClientRpc calls

    // ReSharper disable once UnusedParameter.Global
    [ClientRpc]
    public void ShowAlertClientRpc(string alertText, ClientRpcParams clientRpcParams = default)
    {
        _playerClientSide.ShowAlert(alertText);
    }

    // ReSharper disable once UnusedParameter.Global
    [ClientRpc]
    public void ShowDamageClientRpc(Vector3 position, string damage, ClientRpcParams clientRpcParams = default)
    {
        _playerClientSide.ShowDamage(position, damage);
    }

    [ClientRpc]
    public void EquipsHaveChangedClientRpc()
    {
        //todo: _playerClientSide.ShowDamage(position, damage);
    }

    #endregion

    public void ShowAlertForItemsAddedToInventory(string alertText)
    {
        ShowAlertClientRpc(alertText, _clientRpcParams);
    }

    public void AlertOfInventoryRemovals(int itemsRemoved)
    {
        ShowAlertClientRpc($"Removed {itemsRemoved} items from the inventory after handling message on " + (IsServer ? "server" : "client") + " for " + gameObject.name, _clientRpcParams);
    }

    public void AlertInventoryIsFull()
    {
        ShowAlertClientRpc("Your inventory is at max", _clientRpcParams);
    }

    public void LoadFromPlayerData(PlayerData playerData)
    {
        if (IsServer)
        {
            Username.Value = playerData.Username;
            TextureUrl.Value = playerData.Options?.TextureUrl;
        }

        _loadWasSuccessful = Inventory.ApplyInventory(playerData.Inventory, true);
        SpawnEquippedObjects();
    }

    private void SetNameTag()
    {
        if (IsOwner)
        {
            _nameTag.text = null;
            return;
        }

        _nameTag.text = string.IsNullOrWhiteSpace(Username.Value)
            ? "Player " + NetworkObjectId
            : Username.Value;
    }

    private void SetTexture()
    {
        if (string.IsNullOrWhiteSpace(TextureUrl.Value))
        {
            return;
        }

        //todo: download player texture
        var filePath = TextureUrl.Value;

        var tex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
        tex.LoadImage(System.IO.File.ReadAllBytes(filePath));
        var newMat = new Material(_mainMesh.material.shader)
        {
            mainTexture = tex
        };

        _mainMesh.material = newMat;

        if (IsOwner)
        {
            _leftMesh.material = newMat;
            _rightMesh.material = newMat;
        }
    }

    public void Save()
    {
        if (!IsServer)
        {
            Debug.LogError("Tried to save when not on the server");
        }

        //Debug.Log("Saving player data for " + gameObject.name);

        if (!_loadWasSuccessful)
        {
            Debug.LogWarning("Not saving because the load failed");
            return;
        }

        var saveData = new PlayerData
        {
            Username = Username.Value,
            Inventory = Inventory.GetSaveData()
        };

        //if (saveData.Inventory.Weapons == null || saveData.Inventory.Weapons.Length == 0)
        //{
        //    Debug.LogError("Save data got corrupted. Aborting save!");
        //    return;
        //}

        FullPotential.Assets.Core.Registry.UserRegistry.Save(saveData);
    }

    public void AddToInventory(ItemBase item)
    {
        if (!IsServer)
        {
            Debug.LogError("Inventory Add called on the client!");
            return;
        }

        var invChange = new InventoryAndRemovals { Loot = new[] { item as Loot } };
        Inventory.ApplyInventory(invChange);

        MessageHelper.SendMessageIfNotHost(invChange, nameof(FullPotential.Assets.Core.Networking.MessageType.InventoryChange), OwnerClientId);
    }

    public void SpawnSpellProjectile(Spell activeSpell, Vector3 position, Vector3 direction, ulong clientId)
    {
        if (!IsServer)
        {
            Debug.LogWarning("Tried to spawn a projectile when not on the server");
        }

        //todo: style projectile based on activeSpell

        var spellObject = Instantiate(GameManager.Instance.Prefabs.Combat.Spell, position, Quaternion.identity, GameManager.Instance.MainCanvasObjects.RuntimeObjectsContainer.transform);

        var spellScript = spellObject.GetComponent<SpellBehaviour>();
        spellScript.PlayerClientId = new NetworkVariable<ulong>(clientId);
        spellScript.SpellId = new NetworkVariable<string>(activeSpell.Id);
        spellScript.SpellDirection = new NetworkVariable<Vector3>(direction);

        spellObject.GetComponent<NetworkObject>().Spawn(null, true);
    }

    public void SpawnEquippedObjects()
    {
        for (var slotIndex = 0; slotIndex < Inventory.EquippedObjects.Length; slotIndex++)
        {
            var currentlyInGame = Inventory.EquippedObjects[slotIndex];

            //todo: only destroy if ID is different
            if (currentlyInGame != null)
            {
                Destroy(currentlyInGame);
            }

            var itemId = Inventory.EquipSlots[slotIndex];

            if (string.IsNullOrWhiteSpace(itemId))
            {
                continue;
            }

            var item = Inventory.GetItemWithId<ItemBase>(itemId);

            if (slotIndex == (int)SlotIndexToGameObjectName.LeftHand)
            {
                SpawnItemInHand(slotIndex, item);
            }
            else if (slotIndex == (int)SlotIndexToGameObjectName.RightHand)
            {
                SpawnItemInHand(slotIndex, item, false);
            }
            //todo: other slots
        }
    }

    public void SpawnItemInHand(int index, ItemBase item, bool leftHand = true)
    {
        if (!NetworkManager.Singleton.IsClient)
        {
            Debug.LogError("Tried to spawn a gameobject on a server");
            return;
        }

        if (item is Weapon weapon)
        {
            var registryType = item.RegistryType as IGearWeapon;

            if (registryType == null)
            {
                Debug.LogError("Weapon did not have a RegistryType");
                return;
            }

            GameManager.Instance.TypeRegistry.LoadAddessable(
                weapon.IsTwoHanded ? registryType.PrefabAddressTwoHanded : registryType.PrefabAddress,
                prefab =>
                {
                    var weaponGo = UnityEngine.Object.Instantiate(prefab, InFrontOfPlayer.transform);
                    weaponGo.transform.localEulerAngles = new Vector3(0, 90);
                    weaponGo.transform.localPosition = new Vector3(leftHand ? -0.38f : 0.38f, -0.25f, 1.9f);

                    if (NetworkManager.Singleton.LocalClientId == ClientId.Value) //todo: same as IsOwner?
                    {
                        FullPotential.Assets.Helpers.GameObjectHelper.SetGameLayerRecursive(weaponGo, InFrontOfPlayer.layer);
                    }

                    Inventory.EquippedObjects[index] = weaponGo;
                }
            );
        }
        else
        {
            //todo: implement other items
            Debug.LogWarning($"Not implemented SpawnItemInHand handling for item type {item.GetType().Name} yet");
            Inventory.EquippedObjects[index] = null;
        }
    }

}
