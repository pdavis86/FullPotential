using System;
using UnityEngine;

// ReSharper disable once CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global

public class PlayerToggles : MonoBehaviour
{
    public GameObject _hud;
    public GameObject _crafting;

    public bool HasMenuOpen;

    private PlayerToggles _toggles;
    private bool _doToggle;

    void Awake()
    {
        _toggles = GetComponent<PlayerToggles>();

        _crafting.SetActive(false);
    }

    void Update()
    {
        try
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                _doToggle = true;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    private void FixedUpdate()
    {
        try
        {
            if (_doToggle)
            {
                _doToggle = false;

                _hud.SetActive(!_hud.activeSelf);
                _crafting.SetActive(!_hud.activeSelf);

                _toggles.HasMenuOpen = !_hud.activeSelf;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }

}
