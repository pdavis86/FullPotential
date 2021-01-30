using UnityEngine;

// ReSharper disable once CheckNamespace
// ReSharper disable UnusedMember.Global
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
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _doToggle = true;
        }
    }

    private void FixedUpdate()
    {
        if (_doToggle)
        {
            _doToggle = false;

            _hud.SetActive(!_hud.activeSelf);
            _crafting.SetActive(!_hud.activeSelf);

            _toggles.HasMenuOpen = !_hud.activeSelf;
        }
    }

}
