using FullPotential.Api.Data.Models;

using UnityEngine;

// ReSharper disable UnusedMember.Global

namespace FullPotential.Api.Data
{
    public interface IUserManagement
    {
        Awaitable<string> SignInWithExistingTokenAsync();

        Awaitable<SignInResult> SignInWithPasswordAsync(string username, string password);

        Awaitable<bool> ValidateCredentialsAsync(string username, string token);

        Awaitable<bool> SignOutAsync();
    }
}
