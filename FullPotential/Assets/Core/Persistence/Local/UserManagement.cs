using System;
using System.Threading.Tasks;

using FullPotential.Api.Data;
using FullPotential.Api.Data.Models;

using Newtonsoft.Json.Linq;

using UnityEngine;

namespace FullPotential.Core.Persistence.Local
{
    public class UserManagement : IUserManagement
    {
        private const string dummyToken = "ThisIsNotARealToken";

        public async Awaitable<string> SignInWithExistingTokenAsync()
        {
            await Task.Yield();
            return dummyToken;
        }

        public async Awaitable<SignInResult> SignInWithPasswordAsync(string username, string password)
        {
            await Task.Yield();
            return new SignInResult { Token = dummyToken };
        }

        public async Awaitable<bool> ValidateCredentialsAsync(string username, string token)
        {
            await Task.Yield();
            return true;
        }

        public async Awaitable<bool> SignOutAsync()
        {
            await Task.Yield();
            return true;
        }
    }
}
