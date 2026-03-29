using System;
using System.Collections;

using FullPotential.Api.Data;
using FullPotential.Api.Obsolete;

namespace FullPotential.Core.Persistence.Local
{
    public class UserManagement : IUserManagement
    {
        public PlayerData Load(string username, bool reduced)
        {
            throw new NotImplementedException();
        }

        public void Save(PlayerData playerData)
        {
            throw new NotImplementedException();
        }

        public string SignInWithExistingToken()
        {
            throw new NotImplementedException();
        }

        public IEnumerator SignInWithPasswordEnumerator(string username, string password, Action<string> successCallback, Action<bool> failureCallback)
        {
            throw new NotImplementedException();
        }

        public IEnumerator SignOutEnumerator(Action successCallback, Action failureCallback)
        {
            throw new NotImplementedException();
        }

        public IEnumerator ValidateCredentialsEnumerator(string username, string token, Action successCallback, Action failureCallback)
        {
            throw new NotImplementedException();
        }
    }
}
