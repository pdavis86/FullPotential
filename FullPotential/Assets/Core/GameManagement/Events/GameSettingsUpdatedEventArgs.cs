using System;
using FullPotential.Api.GameManagement;

namespace FullPotential.Core.GameManagement.Events
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
