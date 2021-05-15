using Assets.Core.Data;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable CheckNamespace
// ReSharper disable UnusedType.Global

public class SettingsMenuUi : MonoBehaviour
{
    [SerializeField]
    private Dropdown _languageDropDown;

    [SerializeField]
    private InputField _skinUrlInput;

    private Dictionary<string, string> _cultures;

    private void LoadCultures()
    {
        if (_cultures == null)
        {
            _cultures = GameManager.Instance.Localizer.GetAvailableCultures();
        }
    }

    void Awake()
    {
        LoadCultures();
        _languageDropDown.options.Clear();
        _languageDropDown.AddOptions(_cultures.Select(x => x.Value).ToList());

        //    if (!string.IsNullOrWhiteSpace(GameManager.Instance.PlayerSkinUrl))
        //    {
        //        string filePath;
        //        if (!GameManager.Instance.PlayerSkinUrl.StartsWith("http"))
        //        {
        //            //todo: upload file?
        //            filePath = GameManager.Instance.PlayerSkinUrl;
        //        }
        //        else
        //        {
        //            //todo: download file
        //            filePath = GameManager.Instance.PlayerSkinUrl;
        //        }

        //        if (System.IO.File.Exists(filePath))
        //        {
        //            SetPlayerTexture(filePath);
        //            TextureUri = filePath;
        //        }
        //    }
    }

    public void LoadFromPlayerData(PlayerData playerData)
    {
        LoadCultures();

        int i;
        for (i = 0; i < _cultures.Count; i++)
        {
            if (_cultures.ElementAt(i).Key == playerData.Options.Culture)
            {
                break;
            }
        }
        _languageDropDown.value = i;

        _skinUrlInput.text = playerData.Options.TextureUrl;
    }

    public void SetLanguage(int index) //string value)
    {
        var match = _cultures.ElementAt(index);

        Debug.Log("Setting language to " + match.Value);

        GameManager.SetLastUsedCulture(match.Key);
    }

    public void SaveAndClose()
    {
        GameManager.Instance.LocalPlayer.GetComponent<PlayerSetup>().CmdUpdateTexture(_skinUrlInput.text);
        GameManager.Instance.MainCanvasObjects.HideAllMenus();
    }

}
