using System;
using UnityEngine;

// ReSharper disable once CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Global

public class PlayerToggles : MonoBehaviour
{
    public GameObject Hud;
    public GameObject CraftingUi;

    public bool HasMenuOpen;

    private PlayerToggles _toggles;
    private bool _doToggle;

    void Awake()
    {
        _toggles = GetComponent<PlayerToggles>();

        CraftingUi.SetActive(false);
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

                Hud.SetActive(!Hud.activeSelf);
                CraftingUi.SetActive(!Hud.activeSelf);

                _toggles.HasMenuOpen = !Hud.activeSelf;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }

}
