using Cysharp.Threading.Tasks;

using FullPotential.Api.Data;
using FullPotential.Api.GameManagement.Models;

namespace FullPotential.Core.Persistence.Local
{
    public class UserManagement : IUserManagement
    {
        private const string DummyToken = "ThisIsNotARealToken";

        public UniTask<SignInResult> SignInWithPasswordAsync(string username, string password)
        {
            return UniTask.FromResult(username == password
                ? new SignInResult { UserId = username, Username = username, CharacterId = username, Token = DummyToken }
                : new SignInResult { UserId = username, Username = username, CharacterId = username, IsInvalid = true });
        }

        public UniTask<SignInResult> SignInWithTokenAsync(string username, string token)
        {
            return UniTask.FromResult(token == DummyToken
                ? new SignInResult { UserId = username, Username = username, CharacterId = username, Token = DummyToken }
                : new SignInResult { UserId = username, Username = username, CharacterId = username, IsInvalid = true });
        }

        public UniTask<bool> SignOutAsync()
        {
            return UniTask.FromResult(true);
        }
    }
}
