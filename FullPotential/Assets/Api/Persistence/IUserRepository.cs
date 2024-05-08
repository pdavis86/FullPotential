using FullPotential.Api.Data;

namespace FullPotential.Api.Persistence
{
    public interface IUserRepository
    {
        PlayerData Load(string username, bool reduced);

        void Save(PlayerData playerData);
    }
}