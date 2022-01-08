using FullPotential.Core.Behaviours.GameManagement;
using FullPotential.Core.Behaviours.PlayerBehaviours;
using FullPotential.Core.Extensions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedType.Global
// ReSharper disable ConvertIfStatementToNullCoalescingAssignment

namespace FullPotential.Core.Behaviours.Ui
{
    public class SettingsMenuUi : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField] private Dropdown _resolutionDropDown;
        [SerializeField] private Toggle _fullscreenToggle;
        [SerializeField] private Dropdown _languageDropDown;
        [SerializeField] private InputField _skinUrlInput;
#pragma warning restore 0649

        private Dictionary<string, string> _cultures;
        private int _newCultureIndex = -1;
        private Resolution[] _availableResolutions;

        private void Awake()
        {
            _availableResolutions = Screen.resolutions
                .Where(x => x.refreshRate == 60)
                .Distinct()
                .ToArray();

            _resolutionDropDown.options.Clear();
            _resolutionDropDown.AddOptions(_availableResolutions.Select(x => $"{x.width} x {x.height} ({GetAspectRatio(x.width, x.height)})").ToList());

            _cultures = GameManager.Instance.Localizer.GetAvailableCultures();

            _languageDropDown.options.Clear();
            _languageDropDown.AddOptions(_cultures.Select(x => x.Value).ToList());

            LoadFromUnitySettings();
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

        public void SetResolution(int index)
        {
            var selectedResolution = _availableResolutions[index];
            Screen.SetResolution(selectedResolution.width, selectedResolution.height, _fullscreenToggle.isOn);
        }

        public void ToggleFullscreen(bool isOn)
        {
            Screen.SetResolution(Screen.width, Screen.height, isOn);
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

        private string GetAspectRatio(int width, int height)
        {
            var screenRatio = (float)width / height;

            if (screenRatio >= 1.7)
            {
                return "16:9";
            }

            if (screenRatio >= 1.5)
            {
                return "3:2";
            }

            return "4:3";
        }

        private void LoadFromUnitySettings()
        {
            for (var i = 0; i < _availableResolutions.Length; i++)
            {
                if (_availableResolutions[i].width != Screen.width || _availableResolutions[i].height != Screen.height)
                {
                    continue;
                }

                _resolutionDropDown.value = i;
                break;
            }

            _fullscreenToggle.isOn = Screen.fullScreen;
        }

    }
}
