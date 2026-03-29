using System;
using System.Collections;

using FullPotential.Api.Obsolete;

// ReSharper disable UnusedMember.Global

namespace FullPotential.Api.Data
{
    public interface IUserManagement
    {
        string SignInWithExistingToken();

        IEnumerator SignInWithPasswordEnumerator(string username, string password, Action<string> successCallback, Action<bool> failureCallback);

        IEnumerator SignOutEnumerator(Action successCallback, Action failureCallback);

        IEnumerator ValidateCredentialsEnumerator(string username, string token, Action successCallback, Action failureCallback);

        PlayerData Load(string username, bool reduced);

        void Save(PlayerData playerData);
    }
}
