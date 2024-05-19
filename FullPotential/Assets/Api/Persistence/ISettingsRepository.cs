using FullPotential.Api.Data;
using System;
using FullPotential.Api.GameManagement.Events;

namespace FullPotential.Api.Persistence
{
    public interface ISettingsRepository
    {
        event EventHandler<GameSettingsUpdatedEventArgs> GameSettingsUpdated;

        GameSettings GetOrLoad();

        void Save(GameSettings gameSettings);
    }
}
