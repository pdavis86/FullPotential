using TMPro;
using UnityEngine;
using UnityEngine.Networking;

// ReSharper disable once CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Global

public class PlayerSetup : NetworkBehaviour
{
    [SerializeField] private Camera _playerCamera;
    [SerializeField] private TextMeshProUGUI _nameTag;

    private Camera _sceneCamera;

    private void Start()
    {
        _sceneCamera = Camera.main;

        //todo: let player name themselves
        var networkIdentity = GetComponent<NetworkIdentity>();
        gameObject.name = "Player " + networkIdentity.netId;

        //todo: let players specify a URL to a material

        if (!isLocalPlayer)
        {
            _nameTag.text = gameObject.name;
            gameObject.GetComponent<PlayerController>().enabled = false;
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
    }

    private void OnDisable()
    {
        if (GameManager.Instance.GameObjects.UiHud != null)
        {
            GameManager.Instance.GameObjects.UiHud.SetActive(false);
        }

        if (_sceneCamera != null)
        {
            _sceneCamera.gameObject.SetActive(true);
        }
    }

}
