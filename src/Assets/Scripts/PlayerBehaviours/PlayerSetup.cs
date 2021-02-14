using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

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
    [SerializeField] private TextMeshProUGUI _nameTag;
    [SerializeField] private MeshRenderer _mainMesh;
    [SerializeField] private MeshRenderer _leftMesh;
    [SerializeField] private MeshRenderer _rightMesh;

    [SyncVar]
    public string PlayerName;

    [SyncVar]
    public string TextureUri;

    private Camera _sceneCamera;

    private void Start()
    {
        _sceneCamera = Camera.main;

        gameObject.name = "Player ID " + netId.Value;

        if (!isLocalPlayer)
        {
            gameObject.GetComponent<PlayerController>().enabled = false;

            _nameTag.text = string.IsNullOrWhiteSpace(PlayerName) ? "Player " + netId.Value : PlayerName;

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

        GameManager.Instance.MainCanvasObjects.Hud.SetActive(true);

        var pm = gameObject.AddComponent<PlayerMovement>();
        pm.PlayerCamera = _playerCamera;

        //Done on network manager now
        //ClientScene.RegisterPrefab(_sceneObjects.PrefabSpell);

        _nameTag.text = null;






        if (!string.IsNullOrWhiteSpace(GameManager.Instance.PlayerSkinUrl))
        {
            string filePath;
            if (!GameManager.Instance.PlayerSkinUrl.StartsWith("http"))
            {
                //todo: upload file?
                filePath = GameManager.Instance.PlayerSkinUrl;
            }
            else
            {
                //todo: download file
                filePath = GameManager.Instance.PlayerSkinUrl;
            }

            if (System.IO.File.Exists(filePath))
            {
                SetPlayerTexture(filePath);
                TextureUri = filePath;
            }
        }

        CmdHeresMyJoiningDetails(GameManager.Instance.PlayerName, GameManager.Instance.PlayerSkinUrl);
    }

    private void OnDisable()
    {
        if (isServer)
        {
            var inv = GetComponent<Inventory>();
            //todo: inv.save()
        }

        if (GameManager.Instance.MainCanvasObjects.Hud != null) { GameManager.Instance.MainCanvasObjects.Hud.SetActive(false); }
        if (_sceneCamera != null) { _sceneCamera.gameObject.SetActive(true); }
    }

    void SetPlayerTexture(string playerSkinUri)
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

    [Command]
    void CmdHeresMyJoiningDetails(string playerName, string playerSkinUri)
    {
        if (!string.IsNullOrWhiteSpace(playerName)) { PlayerName = playerName; }
        if (!string.IsNullOrWhiteSpace(playerSkinUri)) { TextureUri = playerSkinUri; }
        RpcSetPlayerDetails(playerName, playerSkinUri);
    }

    [ClientRpc]
    void RpcSetPlayerDetails(string playerName, string playerSkinUri)
    {
        if (!isLocalPlayer)
        {
            _nameTag.text = string.IsNullOrWhiteSpace(playerName) ? "Player " + netId.Value : playerName;
            if (!string.IsNullOrWhiteSpace(playerSkinUri)) { SetPlayerTexture(playerSkinUri); }
        }
    }

}
