using System;

using FullPotential.Api.Data.Models;
using FullPotential.Api.GameManagement.Events;

namespace FullPotential.Api.Data
{
    public interface ISettingsRepository
    {
        event EventHandler<GameSettingsUpdatedEventArgs> GameSettingsUpdated;

        GameSettings Get();

        void Save(GameSettings gameSettings);
    }
}
