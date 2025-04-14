using System;
using FullPotential.Api.Data;

namespace FullPotential.Api.Persistence
{
    [Obsolete]
    public interface IUserRepository
    {
        PlayerData Load(string username, bool reduced);

        void Save(PlayerData playerData);
    }
}