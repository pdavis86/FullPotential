using FullPotential.Api.Data;

namespace FullPotential.Api.Persistence
{
    public interface IUserRepository
    {
        string SignIn(string username, string password);

        string GetUsernameFromToken(string token);

        PlayerData Load(string token, string username, bool reduced);

        void Save(PlayerData playerData);
    }
}