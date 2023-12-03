using FullPotential.Api.Data;

namespace FullPotential.Api.Persistence
{
    public interface ISettingsRepository
    {
        GameSettings Load();

        void Save(GameSettings gameSettings);
    }
}
