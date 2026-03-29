using System;

using FullPotential.Api.Data.Models;

namespace FullPotential.Api.GameManagement.Events
{
    public class GameSettingsUpdatedEventArgs : EventArgs
    {
        public GameSettings UpdatedSettings { get; }

        public GameSettingsUpdatedEventArgs(GameSettings updatedSettings)
        {
            UpdatedSettings = updatedSettings;
        }
    }
}
