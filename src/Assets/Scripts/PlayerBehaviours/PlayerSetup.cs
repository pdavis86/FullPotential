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
    [SerializeField] private MeshRenderer _meshRenderer;

    public string TextureUri;

    private Camera _sceneCamera;

    private void Start()
    {
        _sceneCamera = Camera.main;

        gameObject.name = "Player ID " + netId.Value;

        if (!isLocalPlayer)
        {
            gameObject.GetComponent<PlayerController>().enabled = false;

            //todo: let player name themselves
            _nameTag.text = "Player " + netId.Value;

            return;
        }

        if (_sceneCamera != null)
        {
            _sceneCamera.gameObject.SetActive(false);
        }

        _playerCamera.gameObject.SetActive(true);

        _nameTag.text = null;

        GameManager.Instance.GameObjects.UiHud.SetActive(true);

        var pm = gameObject.AddComponent<PlayerMovement>();
        pm.PlayerCamera = _playerCamera;

        ClientScene.RegisterPrefab(GameManager.Instance.GameObjects.PrefabSpell);

        //todo: let players specify a URL to a texture PNG
        var filePath = @"C:\Users\Paul\Desktop\Untitled.png";
        if (!string.IsNullOrWhiteSpace(filePath) && System.IO.File.Exists(filePath))
        {
            SetPlayerTexture(filePath);
            //todo: upload file
            TextureUri = filePath;
        }

        CmdSendMeAllPlayerMaterials(TextureUri);
    }

    private void OnDisable()
    {
        if (GameManager.Instance?.GameObjects.UiHud != null)
        {
            GameManager.Instance.GameObjects.UiHud.SetActive(false);
        }

        if (_sceneCamera != null)
        {
            _sceneCamera.gameObject.SetActive(true);
        }
    }

    void SetPlayerTexture(string filePath)
    {
        var tex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
        tex.LoadImage(System.IO.File.ReadAllBytes(filePath));
        var newMat = new Material(_meshRenderer.material.shader);
        newMat.mainTexture = tex;
        _meshRenderer.material = newMat;

        if (isLocalPlayer)
        {
            //todo: set texture on hands too
        }
    }

    [Command]
    void CmdSendMeAllPlayerMaterials(string uriToDownloadAndApply)
    {
        if (!string.IsNullOrWhiteSpace(uriToDownloadAndApply))
        {
            TextureUri = uriToDownloadAndApply;
        }

        var playerSetups = GameObject.FindGameObjectsWithTag("Player").Select(x => x.GetComponent<PlayerSetup>());
        foreach (var playerSetup in playerSetups)
        {
            playerSetup.RpcSetPlayerMaterial(playerSetup.TextureUri);
        }
    }

    [ClientRpc]
    void RpcSetPlayerMaterial(string uriToDownloadAndApply)
    {
        if (string.IsNullOrWhiteSpace(uriToDownloadAndApply))
        {
            //Debug.LogError($"No texture for player {gameObject.name}");
            return;
        }

        //Debug.LogError($"Applying texture {uriToDownloadAndApply} to player {gameObject.name}");
        //todo: download file
        var filePath = uriToDownloadAndApply;
        SetPlayerTexture(filePath);
    }

}
