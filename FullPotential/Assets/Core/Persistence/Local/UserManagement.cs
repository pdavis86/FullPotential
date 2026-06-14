using System.Threading.Tasks;

using Cysharp.Threading.Tasks;

using FullPotential.Api.Data;
using FullPotential.Api.GameManagement.Models;

namespace FullPotential.Core.Persistence.Local
{
    public class UserManagement : IUserManagement
    {
        private const string DummyToken = "ThisIsNotARealToken";

        public async UniTask<SignInResult> SignInWithPasswordAsync(string username, string password)
        {
            await Task.Yield();
            return username == password
                ? new SignInResult { UserId = username, Username = username, CharacterId = username, Token = DummyToken }
                : new SignInResult { UserId = username, Username = username, CharacterId = username, IsInvalid = true };
        }

        public async UniTask<SignInResult> SignInWithTokenAsync(string username, string token)
        {
            await Task.Yield();
            return token == DummyToken
                ? new SignInResult { UserId = username, Username = username, CharacterId = username, Token = DummyToken }
                : new SignInResult { UserId = username, Username = username, CharacterId = username, IsInvalid = true };
        }

        public async UniTask<bool> SignOutAsync()
        {
            await Task.Yield();
            return true;
        }
    }
}
