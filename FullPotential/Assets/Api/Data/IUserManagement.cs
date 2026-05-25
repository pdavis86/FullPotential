using Cysharp.Threading.Tasks;

using FullPotential.Api.GameManagement.Models;

namespace FullPotential.Api.Data
{
    public interface IUserManagement
    {
        UniTask<SignInResult> SignInWithPasswordAsync(string username, string password);

        UniTask<bool> ValidateCredentialsAsync(string username, string token);

        UniTask<bool> SignOutAsync();
    }
}
