using FullPotential.Api.Data;
using FullPotential.Api.Persistence;
using FullPotential.Core.Localization;
using UnityEngine;

// ReSharper disable once ClassNeverInstantiated.Global

namespace FullPotential.Core.Persistence
{
    public class SettingsRepository : ISettingsRepository
    {
        public GameSettings Load()
        {
            var path = GetGameSettingsPath();

            GameSettings gameSettings;

            if (System.IO.File.Exists(path))
            {
                gameSettings = JsonUtility.FromJson<GameSettings>(System.IO.File.ReadAllText(path));
            }
            else
            {
                gameSettings = new GameSettings
                {
                    Culture = Localizer.DefaultCulture
                };
            }

            if (gameSettings.LookSensitivity == 0)
            {
                gameSettings.LookSensitivity = 0.2f;
            }

            if (gameSettings.LookSmoothness == 0)
            {
                gameSettings.LookSmoothness = 3;
            }

            return gameSettings;
        }

        public void Save(GameSettings gameSettings)
        {
            System.IO.File.WriteAllText(GetGameSettingsPath(), JsonUtility.ToJson(gameSettings));
        }

        private static string GetGameSettingsPath()
        {
            return Application.persistentDataPath + "/LoadOptions.json";
        }

    }
}
