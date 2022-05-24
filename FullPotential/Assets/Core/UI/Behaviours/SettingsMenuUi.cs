using System.Collections.Generic;
using System.Linq;
using FullPotential.Core.GameManagement;
using FullPotential.Core.Gameplay.Data;
using FullPotential.Core.Localization;
using FullPotential.Core.PlayerBehaviours;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable once UnusedType.Global

namespace FullPotential.Core.Ui.Behaviours
{
    public class SettingsMenuUi : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField] private Dropdown _resolutionDropDown;
        [SerializeField] private Toggle _fullscreenToggle;
        [SerializeField] private Dropdown _languageDropDown;
        [SerializeField] private InputField _skinUrlInput;
        [SerializeField] private Slider _fovSlider;
#pragma warning restore 0649

        private Dictionary<string, string> _cultures;
        private int _newCultureIndex = -1;
        private Resolution[] _availableResolutions;
        
        //Revert variables
        private bool _isRevertRequired;
        private Resolution _revertResolution;
        private bool _revertFullScreen;
        private float _revertFieldOfView;

        #region Event handlers

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            _availableResolutions = Screen.resolutions
                .Where(x => x.refreshRate == 60)
                .Distinct()
                .ToArray();

            _resolutionDropDown.options.Clear();
            _resolutionDropDown.AddOptions(_availableResolutions.Select(x => $"{x.width} x {x.height} ({GetAspectRatio(x.width, x.height)})").ToList());

            _cultures = GameManager.Instance.GetService<Localizer>().GetAvailableCultures();

            _languageDropDown.options.Clear();
            _languageDropDown.AddOptions(_cultures.Select(x => x.Value).ToList());
        }

        // ReSharper disable once UnusedMember.Local
        private void OnEnable()
        {
            LoadApplicationSettings();
            LoadGameSettings();
            LoadPlayerSettings();

            _isRevertRequired = true;
        }

        // ReSharper disable once UnusedMember.Local
        private void OnDisable()
        {
            RevertUnsavedSettings();
        }

        #endregion

        // ReSharper disable once UnusedMember.Global
        public void SetResolution(int index)
        {
            var selectedResolution = _availableResolutions[index];
            Screen.SetResolution(selectedResolution.width, selectedResolution.height, _fullscreenToggle.isOn);
        }

        // ReSharper disable once UnusedMember.Global
        public void ToggleFullscreen(bool isOn)
        {
            Screen.SetResolution(Screen.width, Screen.height, isOn);
        }

        // ReSharper disable once UnusedMember.Global
        public void SetFieldOfView(float degrees)
        {
            Camera.main.fieldOfView = degrees;
        }

        // ReSharper disable once UnusedMember.Global
        public void SetLanguage(int index)
        {
            _newCultureIndex = index;
        }

        // ReSharper disable once UnusedMember.Global
        public async void SaveAndClose()
        {
            if (_newCultureIndex > -1)
            {
                var match = _cultures.ElementAt(_newCultureIndex);
                await GameManager.Instance.SetCultureAsync(match.Key);
            }

            GameManager.Instance.AppOptions.FieldOfView = Camera.main.fieldOfView;

            var playerSettings = new PlayerSettings
            {
                TextureUrl = _skinUrlInput.text
            };

            GameManager.Instance.LocalGameDataStore.GameObject.GetComponent<PlayerActions>().UpdatePlayerSettings(playerSettings);

            _isRevertRequired = false;

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

        private void LoadApplicationSettings()
        {
            for (var i = 0; i < _availableResolutions.Length; i++)
            {
                if (_availableResolutions[i].width != Screen.width || _availableResolutions[i].height != Screen.height)
                {
                    continue;
                }

                _resolutionDropDown.value = i;
                _revertResolution = _availableResolutions[i];
                break;
            }

            _revertFullScreen = Screen.fullScreen;
            _fullscreenToggle.isOn = Screen.fullScreen;
        }

        private void LoadGameSettings()
        {
            _revertFieldOfView = Camera.main.fieldOfView;
            _fovSlider.value = Camera.main.fieldOfView;

            var culture = GameManager.Instance.AppOptions.Culture;
            int i;
            for (i = 0; i < _cultures.Count; i++)
            {
                if (_cultures.ElementAt(i).Key == culture)
                {
                    break;
                }
            }
            _languageDropDown.value = i;
        }

        private void LoadPlayerSettings()
        {
            var playerState = GameManager.Instance.LocalGameDataStore.GameObject.GetComponent<PlayerState>();
            _skinUrlInput.text = playerState.TextureUrl;
        }

        private void RevertUnsavedSettings()
        {
            if (!_isRevertRequired)
            {
                return;
            }

            //Application settings
            Screen.SetResolution(_revertResolution.width, _revertResolution.height, _revertFullScreen);

            //Game settings
            Camera.main.fieldOfView = _revertFieldOfView;
        }

    }
}
