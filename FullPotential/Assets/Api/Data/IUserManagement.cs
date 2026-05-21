using Cysharp.Threading.Tasks;

using FullPotential.Api.Data.Models;

namespace FullPotential.Api.Data
{
    public interface IUserManagement
    {
        UniTask<SignInResult> SignInWithPasswordAsync(string username, string password);

        UniTask<bool> ValidateCredentialsAsync(string username, string token);

        UniTask<bool> SignOutAsync();
    }
}
