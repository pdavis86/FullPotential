using Assets.Scripts.Data;
using TMPro;
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

public class PlayerSetup : NetworkBehaviour
{
    [SerializeField] private Camera _playerCamera;
    [SerializeField] private Camera _inFrontOfPlayerCamera;
    [SerializeField] private TextMeshProUGUI _nameTag;
    [SerializeField] private MeshRenderer _mainMesh;
    [SerializeField] private MeshRenderer _leftMesh;
    [SerializeField] private MeshRenderer _rightMesh;

    //todo: when is this false?
    [SerializeField] private bool _debugging = true;

    [SyncVar]
    public string Username;

    [SyncVar]
    public string TextureUri;

    private Camera _sceneCamera;
    private Inventory _inventory;

    private void Start()
    {
        _sceneCamera = Camera.main;
        _inventory = GetComponent<Inventory>();

        gameObject.name = "Player ID " + netId.Value;

        if (!isLocalPlayer)
        {
            gameObject.GetComponent<PlayerController>().enabled = false;

            _nameTag.text = string.IsNullOrWhiteSpace(Username) ? "Player " + netId.Value : Username;

            if (!string.IsNullOrWhiteSpace(TextureUri))
            {
                SetPlayerTexture(TextureUri);
            }

            return;
        }

        if (_sceneCamera != null)
        {
            _sceneCamera.gameObject.SetActive(false);
        }

        _playerCamera.gameObject.SetActive(true);
        _inFrontOfPlayerCamera.gameObject.SetActive(true);

        GameManager.Instance.MainCanvasObjects.Hud.SetActive(true);

        var pm = gameObject.AddComponent<PlayerMovement>();
        pm.PlayerCamera = _playerCamera;

        //Done on network manager now
        //ClientScene.RegisterPrefab(_sceneObjects.PrefabSpell);

        connectionToServer.RegisterHandler(Assets.Scripts.Networking.MessageIds.LoadPlayerData, OnLoadPlayerData);

        _nameTag.text = null;

        //if (!string.IsNullOrWhiteSpace(GameManager.Instance.PlayerSkinUrl))
        //{
        //    string filePath;
        //    if (!GameManager.Instance.PlayerSkinUrl.StartsWith("http"))
        //    {
        //        //todo: upload file?
        //        filePath = GameManager.Instance.PlayerSkinUrl;
        //    }
        //    else
        //    {
        //        //todo: download file
        //        filePath = GameManager.Instance.PlayerSkinUrl;
        //    }

        //    if (System.IO.File.Exists(filePath))
        //    {
        //        SetPlayerTexture(filePath);
        //        TextureUri = filePath;
        //    }
        //}

        GameManager.Instance.LocalPlayer = gameObject;
        CmdHeresMyJoiningDetails(GameManager.Instance.PlayerName, GameManager.Instance.PlayerSkinUrl);
    }

    private void OnDisable()
    {
        Save();

        if (GameManager.Instance.MainCanvasObjects.Hud != null) { GameManager.Instance.MainCanvasObjects.Hud.SetActive(false); }
        if (_sceneCamera != null) { _sceneCamera.gameObject.SetActive(true); }
    }

    private void SetPlayerTexture(string playerSkinUri)
    {
        //todo: download file
        var filePath = playerSkinUri;

        var tex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
        tex.LoadImage(System.IO.File.ReadAllBytes(filePath));
        var newMat = new Material(_mainMesh.material.shader);
        newMat.mainTexture = tex;

        _mainMesh.material = newMat;

        if (isLocalPlayer)
        {
            _leftMesh.material = newMat;
            _rightMesh.material = newMat;
        }
    }

    [ClientRpc]
    private void RpcSetPlayerDetails(string playerName, string playerSkinUri)
    {
        if (!isLocalPlayer)
        {
            _nameTag.text = string.IsNullOrWhiteSpace(playerName) ? "Player " + netId.Value : playerName;
        }

        if (!string.IsNullOrWhiteSpace(playerSkinUri)) { SetPlayerTexture(playerSkinUri); }
    }

    private string GetPlayerSavePath()
    {
        //todo configurable server save path
        //todo: change file path
        return @"D:\temp\playerguid.json";
    }

    [Command]
    private void CmdHeresMyJoiningDetails(string playerName, string playerSkinUri)
    {
        if (!string.IsNullOrWhiteSpace(playerName)) { Username = playerName; }
        if (!string.IsNullOrWhiteSpace(playerSkinUri)) { TextureUri = playerSkinUri; }

        //todo: this can be merged into Load()
        RpcSetPlayerDetails(playerName, playerSkinUri);

        var filePath = GetPlayerSavePath();
        if (System.IO.File.Exists(filePath))
        {
            var loadJson = System.IO.File.ReadAllText(filePath);

            Load(loadJson);

            if (!isLocalPlayer)
            {
                connectionToClient.Send(Assets.Scripts.Networking.MessageIds.LoadPlayerData, new StringMessage(loadJson));
            }
        }
    }

    private void OnLoadPlayerData(NetworkMessage netMsg)
    {
        Load(netMsg.ReadMessage<StringMessage>().value);
    }

    private void Load(string loadJson)
    {
        var loadData = JsonUtility.FromJson<PlayerData>(loadJson);

        _inventory.ApplyChanges(loadData.Inventory, true);

        //todo: load other player data into correct objects
    }

    [Server]
    private void Save()
    {
        //Debug.Log("Saving player data for " + gameObject.name);

        var saveData = new PlayerData
        {
            Inventory = _inventory.GetSaveData()
        };

        //todo: figure out how to avoid saving after a load failed
        if (saveData.Inventory.Weapons == null || saveData.Inventory.Weapons.Length == 0)
        {
            Debug.LogError("Save data got corrupted. Aborting save!");
            return;
        }

        var saveJson = JsonUtility.ToJson(saveData, _debugging);
        System.IO.File.WriteAllText(GetPlayerSavePath(), saveJson);
    }

}
