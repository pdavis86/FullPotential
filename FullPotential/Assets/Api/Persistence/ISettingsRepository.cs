using FullPotential.Api.GameManagement;

namespace FullPotential.Api.Persistence
{
    public interface ISettingsRepository
    {
        GameSettings Load();

        void Save(GameSettings gameSettings);
    }
}
