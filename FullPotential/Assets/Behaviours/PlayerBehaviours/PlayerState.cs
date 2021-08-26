using FullPotential.Assets.Core.Data;
using FullPotential.Assets.Core.Registry.Types;
using FullPotential.Assets.Core.Storage;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
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

        if (!IsServer)
        {
            CustomMessagingManager.RegisterNamedMessageHandler(nameof(FullPotential.Assets.Core.Networking.MessageType.LoadPlayerData), OnLoadPlayerData);
            RequestPlayerDataServerRpc();
        }
    }

    private void OnDisable()
    {
        Username.OnValueChanged -= OnUsernameChanged;
        TextureUrl.OnValueChanged -= OnTextureChanged;
    }

    private void OnTextureChanged(string previousValue, string newValue)
    {
        SetTexture();
    }

    private void OnUsernameChanged(string previousValue, string newValue)
    {
        SetNameTag();
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
        LoadFromPlayerData(playerData);
    }

    #endregion

    #region ServerRpc calls

    [ServerRpc]
    public void RequestPlayerDataServerRpc()
    {
        var playerData = FullPotential.Assets.Core.Registry.UserRegistry.LoadFromUsername(Username.Value);

        //Debug.LogError("Sending playerData to clientId " + ClientId);

        var json = JsonUtility.ToJson(playerData);
        //todo: use compression? - var jsonCompressed = Assets.Core.Helpers.CompressionHelper.CompressString(json);

        var stream = PooledNetworkBuffer.Get();
        using (PooledNetworkWriter writer = PooledNetworkWriter.Get(stream))
        {
            writer.WriteString(json);
            CustomMessagingManager.SendNamedMessage(nameof(FullPotential.Assets.Core.Networking.MessageType.LoadPlayerData), OwnerClientId, stream);
        }
    }

    [ServerRpc]
    public void UpdatePlayerSettingsServerRpc(string textureUrl)
    {
        TextureUrl.Value = textureUrl;
    }

    [ServerRpc]
    public void CastSpellServerRpc(bool leftHand, Vector3 position, Vector3 direction, ServerRpcParams serverRpcParams = default)
    {
        var activeSpell = Inventory.GetSpellInHand(leftHand);

        if (activeSpell == null)
        {
            return;
        }

        switch (activeSpell.Targeting)
        {
            case FullPotential.Assets.Core.Spells.Targeting.Projectile _:
                SpawnSpellProjectile(activeSpell, leftHand, position, direction, serverRpcParams.Receive.SenderClientId);
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

        Debug.Log($"Trying to interact with {interactable.name}");

        var distance = Vector3.Distance(gameObject.transform.position, interactable.transform.position);
        if (distance <= interactable.Radius)
        {
            interactable.OnInteract(serverRpcParams.Receive.SenderClientId);
        }
    }

    [ServerRpc]
    public void CraftItemServerRpc(string[] componentIds, string categoryName, string craftableTypeName, bool isTwoHanded, string itemName)
    {
        var components = Inventory.GetComponentsFromIds(componentIds);

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

        if (Inventory.ValidateIsCraftable(componentIds, craftedItem).Any())
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

        Inventory.ApplyInventoryAndRemovals(invChange);

        var json = JsonUtility.ToJson(invChange);

        var stream = PooledNetworkBuffer.Get();
        using (PooledNetworkWriter writer = PooledNetworkWriter.Get(stream))
        {
            writer.WriteString(json);
            CustomMessagingManager.SendNamedMessage(nameof(FullPotential.Assets.Core.Networking.MessageType.InventoryChange), OwnerClientId, stream);
        }
    }

    //[ServerRpc]
    //private void SetItemToSlotServerRpc(string slotName, string itemId)
    //{
    //    Inventory.SetItemToSlot(slotName, itemId);
    //}

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
        Username.Value = playerData.Username;
        TextureUrl.Value = playerData.Options.TextureUrl;
        _loadWasSuccessful = Inventory.ApplyInventory(playerData.Inventory, true);
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
            Debug.LogWarning("Tried to save when not on the server");
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

    private void SpawnSpellProjectile(Spell activeSpell, bool leftHand, Vector3 position, Vector3 direction, ulong clientId)
    {
        if (!IsServer)
        {
            Debug.LogWarning("Tried to spawn a projectile when not on the server");
        }

        //todo: style projectile based on activeSpell

        var startPos = position + gameObject.transform.forward + new Vector3(leftHand ? -0.15f : 0.15f, -0.1f, 0);
        var spellObject = Instantiate(GameManager.Instance.Prefabs.Combat.Spell, startPos, Quaternion.identity, GameManager.Instance.MainCanvasObjects.RuntimeObjectsContainer.transform);

        var spellScript = spellObject.GetComponent<SpellBehaviour>();
        spellScript.PlayerClientId = new NetworkVariable<ulong>(clientId);
        spellScript.SpellId = new NetworkVariable<string>(activeSpell.Id);
        spellScript.SpellDirection = new NetworkVariable<Vector3>(direction);

        spellObject.GetComponent<NetworkObject>().Spawn(null, true);
    }

}
