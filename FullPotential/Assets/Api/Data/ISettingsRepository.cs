using System;

using FullPotential.Api.GameManagement.Events;
using FullPotential.Api.GameManagement.Models;

namespace FullPotential.Api.Data
{
    public interface ISettingsRepository
    {
        event EventHandler<GameSettingsUpdatedEventArgs> GameSettingsUpdated;

        GameSettings Get();

        void Save(GameSettings gameSettings);
    }
}
