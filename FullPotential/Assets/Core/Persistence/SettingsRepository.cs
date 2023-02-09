using FullPotential.Api.GameManagement;
using FullPotential.Api.Persistence;
using FullPotential.Api.Utilities.Extensions;
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

            //todo: zzz v0.5 - Remove GameSettings.Username backwards compat
#pragma warning disable CS0618
            if (!gameSettings.Username.IsNullOrWhiteSpace() &&
                gameSettings.LastSigninUsername.IsNullOrWhiteSpace())
            {
                gameSettings.LastSigninUsername = gameSettings.Username;
                gameSettings.Username = null;
            }
#pragma warning restore CS0618

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
