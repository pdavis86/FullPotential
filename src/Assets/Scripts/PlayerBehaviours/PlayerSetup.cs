using UnityEngine;
using UnityEngine.Networking;

// ReSharper disable once CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Global

#pragma warning disable CS0618 // Type or member is obsolete
public class PlayerSetup : NetworkBehaviour
#pragma warning restore CS0618 // Type or member is obsolete
{
    private Camera _sceneCamera;

    private void Start()
    {
        var playerCamera = transform.Find("PlayerCamera").GetComponent<Camera>();

        if (!isLocalPlayer)
        {
            playerCamera.gameObject.SetActive(false);

            //playerCamera.GetComponent<Camera>().enabled = false;
            //playerCamera.GetComponent<AudioListener>().enabled = false;

            return;
        }

        GameManager.Instance.GameObjects.UiHud.SetActive(true);

        var pp = gameObject.AddComponent<PlayerController>();
        pp.PlayerCamera = playerCamera;

        var pm = gameObject.AddComponent<PlayerMovement>();
        pm.PlayerCamera = playerCamera;

        //todo: maybe merge this with PlayerController?
        var pc = gameObject.AddComponent<PlayerCast>();
        pc.PlayerCamera = playerCamera;

        _sceneCamera = Camera.main;
        if (_sceneCamera != null)
        {
            _sceneCamera.gameObject.SetActive(false);
        }
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
