using UnityEngine;
using UnityEngine.UI;

// ReSharper disable CheckNamespace
// ReSharper disable UnusedType.Global

public class SettingsMenuUi : MonoBehaviour
{
    [SerializeField]
    private Dropdown _languageDropDown;

    void Awake()
    {
        _languageDropDown.options.Clear();
        _languageDropDown.AddOptions(GameManager.Instance.Localizer.GetAvailableCultures());

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

    public void SetLanguage(string value)
    {
        Debug.Log("Setting language to " + value);
        GameManager.SetLastUsedCulture(value);
    }

}
