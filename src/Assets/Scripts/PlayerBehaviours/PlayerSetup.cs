using Assets.Core.Data;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Compiler

public class PlayerSetup : NetworkBehaviour
{
    [SerializeField] private Camera _playerCamera;
    [SerializeField] private Camera _inFrontOfPlayerCamera;
    [SerializeField] private TextMeshProUGUI _nameTag;
    [SerializeField] private MeshRenderer _mainMesh;
    [SerializeField] private MeshRenderer _leftMesh;
    [SerializeField] private MeshRenderer _rightMesh;

    [SyncVar]
    public string Username;

    private Camera _sceneCamera;
    private PlayerInventory _inventory;
    private bool _loadWasSuccessful;
    private string _textureUri;

    private void Start()
    {
        _inventory = GetComponent<PlayerInventory>();

        gameObject.name = "Player ID " + netId.Value;

        if (!isLocalPlayer)
        {
            gameObject.GetComponent<PlayerController>().enabled = false;
            _nameTag.text = string.IsNullOrWhiteSpace(Username) ? "Player " + netId.Value : Username;
            return;
        }

        GameManager.Instance.LocalPlayer = gameObject;
        _nameTag.text = null;

        _sceneCamera = Camera.main;
        if (_sceneCamera != null)
        {
            _sceneCamera.gameObject.SetActive(false);
        }

        _playerCamera.gameObject.SetActive(true);
        _inFrontOfPlayerCamera.gameObject.SetActive(true);

        var pm = gameObject.AddComponent<PlayerMovement>();
        pm.PlayerCamera = _playerCamera;

        connectionToServer.RegisterHandler(Assets.Core.Networking.MessageIds.LoadPlayerData, OnLoadPlayerData);

        GameManager.Instance.MainCanvasObjects.Hud.SetActive(true);

        CmdHeresMyJoiningDetails(GameManager.Instance.Username);
    }

    private void OnDisable()
    {
        if (isServer)
        {
            Save();
        }

        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.MainCanvasObjects.Hud != null) { GameManager.Instance.MainCanvasObjects.Hud.SetActive(false); }
            if (_sceneCamera != null) { _sceneCamera.gameObject.SetActive(true); }
        }
    }

    private void SetPlayerTexture(string playerSkinUri)
    {
        //todo: download player texture
        var filePath = playerSkinUri;

        var tex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
        tex.LoadImage(System.IO.File.ReadAllBytes(filePath));
        var newMat = new Material(_mainMesh.material.shader)
        {
            mainTexture = tex
        };

        _mainMesh.material = newMat;

        if (isLocalPlayer)
        {
            _leftMesh.material = newMat;
            _rightMesh.material = newMat;
        }
    }

    [ClientRpc]
    private void RpcSetPlayerDetails(string username, string playerSkinUri)
    {
        if (!isLocalPlayer)
        {
            _nameTag.text = string.IsNullOrWhiteSpace(username) ? "Player " + netId.Value : username;
        }

        if (!string.IsNullOrWhiteSpace(playerSkinUri))
        {
            SetPlayerTexture(playerSkinUri);
        }
    }

    private string GetPlayerSavePath()
    {
        //todo: filename should be their user ID, not the device ID
        return System.IO.Path.Combine(Application.persistentDataPath, SystemInfo.deviceUniqueIdentifier + ".json");
    }

    [Command]
    private void CmdHeresMyJoiningDetails(string username)
    {
        if (!string.IsNullOrWhiteSpace(username))
        {
            Username = username;
        }

        //if (!string.IsNullOrWhiteSpace(playerSkinUri))
        //{
        //    TextureUri = playerSkinUri;
        //}

        var filePath = GetPlayerSavePath();
        if (System.IO.File.Exists(filePath))
        {
            var loadJson = System.IO.File.ReadAllText(filePath);

            LoadFromJson(loadJson);

            if (!isLocalPlayer)
            {
                connectionToClient.Send(Assets.Core.Networking.MessageIds.LoadPlayerData, new StringMessage(loadJson));
            }
        }
        else
        {
            LoadFromPlayerData(new PlayerData());
        }

        RpcSetPlayerDetails(username, _textureUri);
    }

    private void OnLoadPlayerData(NetworkMessage netMsg)
    {
        LoadFromJson(netMsg.ReadMessage<StringMessage>().value);
    }

    private void LoadFromJson(string loadJson)
    {
        LoadFromPlayerData(JsonUtility.FromJson<PlayerData>(loadJson));
    }

    private void LoadFromPlayerData(PlayerData loadData)
    {
        //todo: set texture URI
        _textureUri = string.Empty;
        _loadWasSuccessful = _inventory.ApplyChanges(loadData?.Inventory, true);
    }

    [Server]
    private void Save()
    {
        //Debug.Log("Saving player data for " + gameObject.name);

        if (!_loadWasSuccessful)
        {
            Debug.LogWarning("Not saving because the load failed");
            return;
        }

        var saveData = new PlayerData
        {
            Inventory = _inventory.GetSaveData()
        };

        //if (saveData.Inventory.Weapons == null || saveData.Inventory.Weapons.Length == 0)
        //{
        //    Debug.LogError("Save data got corrupted. Aborting save!");
        //    return;
        //}

        var prettyPrint = Debug.isDebugBuild;
        var saveJson = JsonUtility.ToJson(saveData, prettyPrint);
        System.IO.File.WriteAllText(GetPlayerSavePath(), saveJson);
    }

}
