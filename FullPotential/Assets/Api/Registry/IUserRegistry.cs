﻿using FullPotential.Api.Gameplay.Data;

namespace FullPotential.Api.Registry
{
    public interface IUserRegistry
    {
        string SignIn(string username, string password);
        string GetUsernameFromToken(string token);
        PlayerData Load(string token, string username, bool reduced);
        void Save(PlayerData playerData);
    }
}