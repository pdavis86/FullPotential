using Assets.Scripts.Networking;
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

public class PlayerSetup : NetworkBehaviour2
{
    [SerializeField] private TextMeshProUGUI _nameTag;
    [SerializeField] private Camera _playerCamera;

    private Camera _sceneCamera;

    private void Start()
    {
        _sceneCamera = Camera.main;

        //todo: let player name themselves
#pragma warning disable CS0618 // Type or member is obsolete
        var networkIdentity = GetComponent<NetworkIdentity>();
#pragma warning restore CS0618 // Type or member is obsolete
        gameObject.name = "Player " + networkIdentity.netId;

        //todo: let players specify a URL to a material

        if (!isLocalPlayer)
        {
            _nameTag.text = gameObject.name;
            return;
        }

        if (_sceneCamera != null)
        {
            _sceneCamera.gameObject.SetActive(false);
        }

        _playerCamera.gameObject.SetActive(true);

        _nameTag.text = null;

        GameManager.Instance.GameObjects.UiHud.SetActive(true);

        gameObject.AddComponent<PlayerController>();
        gameObject.AddComponent<PlayerMovement>();

        //todo: maybe merge this with PlayerController?
        gameObject.AddComponent<PlayerCast>();
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
