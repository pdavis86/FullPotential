using Assets.Core.Data;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using MLAPI.Serialization.Pooled;
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

public class PlayerState : NetworkBehaviour
{
#pragma warning disable 0649
    [SerializeField] private Behaviour[] _behavioursToDisable;
    [SerializeField] private TextMeshProUGUI _nameTag;
    [SerializeField] private MeshRenderer _mainMesh;
    [SerializeField] private MeshRenderer _leftMesh;
    [SerializeField] private MeshRenderer _rightMesh;
#pragma warning restore 0649

    public readonly NetworkVariable<ulong> ClientId = new NetworkVariable<ulong>();
    public readonly NetworkVariable<string> Username = new NetworkVariable<string>();
    public readonly NetworkVariable<string> TextureUrl = new NetworkVariable<string>();

    private PlayerInventory _inventory = new PlayerInventory();
    private bool _loadWasSuccessful;

    //todo: public event Action NetworkReadied;

    #region Event handlers

    private void Awake()
    {
        Username.OnValueChanged += OnUsernameChanged;
        TextureUrl.OnValueChanged += OnTextureChanged;

        if (IsLocalPlayer)
        {
            GameManager.Instance.DataStore.LocalPlayer = gameObject;
        }
    }

    private void Start()
    {
        gameObject.name = "Player ID " + NetworkObjectId;

        if (!IsLocalPlayer)
        {
            foreach (var comp in _behavioursToDisable)
            {
                comp.enabled = false;
            }

            //todo: these should not be necessary calls for the client, right?
            //SetNameTag();
            //SetTexture();

            return;
        }

        CustomMessagingManager.RegisterNamedMessageHandler(nameof(Assets.Core.Networking.MessageType.LoadPlayerData), OnLoadPlayerData);
        RequestPlayerDataServerRpc();
    }

    private void OnDisable()
    {
        Username.OnValueChanged -= OnUsernameChanged;
        TextureUrl.OnValueChanged -= OnTextureChanged;

        if (IsServer)
        {
            Save();
        }
    }

    private void OnTextureChanged(string previousValue, string newValue)
    {
        SetTexture();
    }

    private void OnUsernameChanged(string previousValue, string newValue)
    {
        SetNameTag();
    }

    #endregion

    [ServerRpc]
    public void RequestPlayerDataServerRpc()
    {
        var playerData = Assets.Core.Registry.UserRegistry.LoadFromUsername(Username.Value);

        //Debug.LogError("Sending playerData to clientId " + ClientId);

        var json = JsonUtility.ToJson(playerData);
        //todo: use compression? - var jsonCompressed = Assets.Core.Helpers.CompressionHelper.CompressString(json);

        var stream = PooledNetworkBuffer.Get();
        using (PooledNetworkWriter writer = PooledNetworkWriter.Get(stream))
        {
            writer.WriteString(json);
            CustomMessagingManager.SendNamedMessage(nameof(Assets.Core.Networking.MessageType.LoadPlayerData), OwnerClientId, stream);
        }
    }

    [ServerRpc]
    public void UpdatePlayerSettingsServerRpc(string textureUrl)
    {
        TextureUrl.Value = textureUrl;
    }

    public void LoadFromPlayerData(PlayerData playerData)
    {
        Username.Value = playerData.Username;
        TextureUrl.Value = playerData.Options.TextureUrl;
        _loadWasSuccessful = _inventory.ApplyInventory(playerData.Inventory, true);
    }

    private void SetNameTag()
    {
        if (IsLocalPlayer)
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

        if (IsLocalPlayer)
        {
            _leftMesh.material = newMat;
            _rightMesh.material = newMat;
        }
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

    private void Save()
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
            Inventory = _inventory.GetSaveData()
        };

        //if (saveData.Inventory.Weapons == null || saveData.Inventory.Weapons.Length == 0)
        //{
        //    Debug.LogError("Save data got corrupted. Aborting save!");
        //    return;
        //}

        Assets.Core.Registry.UserRegistry.Save(saveData);
    }

}
