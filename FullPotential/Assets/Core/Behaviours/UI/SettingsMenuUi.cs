using FullPotential.Assets.Core.Extensions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedType.Global
// ReSharper disable ConvertIfStatementToNullCoalescingAssignment

public class SettingsMenuUi : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private Dropdown _languageDropDown;
    [SerializeField] private InputField _skinUrlInput;
#pragma warning restore 0649

    private Dictionary<string, string> _cultures;
    private int _newCultureIndex = -1;

    private void Awake()
    {
        _cultures = GameManager.Instance.Localizer.GetAvailableCultures();

        _languageDropDown.options.Clear();
        _languageDropDown.AddOptions(_cultures.Select(x => x.Value).ToList());
    }

    private void OnEnable()
    {
        var culture = GameManager.GetLastUsedCulture().OrIfNullOrWhitespace(GameManager.Instance.Localizer.CurrentCulture);
        int i;
        for (i = 0; i < _cultures.Count; i++)
        {
            if (_cultures.ElementAt(i).Key == culture)
            {
                break;
            }
        }
        _languageDropDown.value = i;

        var playerState = GameManager.Instance.DataStore.LocalPlayer.GetComponent<PlayerState>();
        _skinUrlInput.text = playerState.TextureUrl.Value.ToString();
    }

    public void SetLanguage(int index)
    {
        _newCultureIndex = index;
    }

    public async void SaveAndClose()
    {
        if (_newCultureIndex > -1)
        {
            var match = _cultures.ElementAt(_newCultureIndex);
            await GameManager.Instance.SetCultureAsync(match.Key);
        }

        GameManager.Instance.DataStore.LocalPlayer.GetComponent<PlayerActions>().UpdatePlayerSettingsServerRpc(_skinUrlInput.text);

        GameManager.Instance.MainCanvasObjects.HideAllMenus();
    }

}
