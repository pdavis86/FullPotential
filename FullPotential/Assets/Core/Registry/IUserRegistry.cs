using FullPotential.Core.Gameplay.Data;

namespace FullPotential.Core.Registry
{
    public interface IUserRegistry
    {
        string SignIn(string username, string password);
        bool ValidateToken(string token);
        PlayerData Load(string token, string username, bool reduced);
        void Save(PlayerData playerData);
    }
}