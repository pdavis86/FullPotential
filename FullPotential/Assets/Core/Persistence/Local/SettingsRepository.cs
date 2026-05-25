using System;

using FullPotential.Api.Data;
using FullPotential.Api.Data.Models;
using FullPotential.Api.GameManagement.Events;
using FullPotential.Api.Utilities.Extensions;
using FullPotential.Core.Localization;

using UnityEngine;

// ReSharper disable once ClassNeverInstantiated.Global

namespace FullPotential.Core.Persistence.Local
{
    public class SettingsRepository : ISettingsRepository
    {
        private GameSettings _gameSettings;

        public event EventHandler<GameSettingsUpdatedEventArgs> GameSettingsUpdated;

        public GameSettings Get()
        {
            return _gameSettings ??= Load();
        }

        public void Save(GameSettings gameSettings)
        {
            System.IO.File.WriteAllText(GetGameSettingsPath(), JsonUtility.ToJson(gameSettings, true));
            _gameSettings = gameSettings;
            GameSettingsUpdated?.Invoke(this, new GameSettingsUpdatedEventArgs(gameSettings));
        }

        private static string GetGameSettingsPath()
        {
            return Application.persistentDataPath + "/LoadOptions.json";
        }

        private GameSettings Load()
        {
            var path = GetGameSettingsPath();

            var gameSettings = System.IO.File.Exists(path)
                ? JsonUtility.FromJson<GameSettings>(System.IO.File.ReadAllText(path))
                : new GameSettings();

            SetDefaultsIfMissing(gameSettings);

            GameSettingsUpdated?.Invoke(this, new GameSettingsUpdatedEventArgs(gameSettings));

            return gameSettings;
        }

        private void SetDefaultsIfMissing(GameSettings gameSettings)
        {
            if (gameSettings.Culture.IsNullOrWhiteSpace())
            {
                gameSettings.Culture = Localizer.DefaultCulture;
            }

            if (gameSettings.LookSensitivity == 0)
            {
                gameSettings.LookSensitivity = 0.2f;
            }

            if (gameSettings.LookSmoothness == 0)
            {
                gameSettings.LookSmoothness = 3;
            }
        }
    }
}
