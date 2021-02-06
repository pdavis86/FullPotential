using System.Collections;
using System.Collections.Generic;
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

        ObjectAccess.Instance.UiHud.SetActive(true);

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
        if (ObjectAccess.Instance.UiHud != null)
        {
            ObjectAccess.Instance.UiHud.SetActive(false);
        }

        if (_sceneCamera != null)
        {
            _sceneCamera.gameObject.SetActive(true);
        }
    }
}
