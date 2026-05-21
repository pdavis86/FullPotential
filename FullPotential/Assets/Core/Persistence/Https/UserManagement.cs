using Cysharp.Threading.Tasks;

using FullPotential.Api.Data;
using FullPotential.Api.Data.Models;
using FullPotential.Api.GameManagement.Models;

using UnityEngine;

using UnityEngine.Networking;

namespace FullPotential.Core.Persistence.Https
{
    public class UserManagement : HttpsPersistenceBase, IUserManagement
    {
        public UserManagement(ISettingsRepository settingsRepository)
            : base(settingsRepository)
        {
        }

        public async UniTask<SignInResult> SignInWithPasswordAsync(string username, string password)
        {
            var data = JsonUtility.ToJson(new Credentials
            {
                Username = username,
                PasswordOrToken = password
            });

            using (var request = UnityWebRequest.Post(BaseAddress + "User/SignInWithPassword", data, JsonContentType))
            {
                await request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    LogFailure(request);
                    return new SignInResult();
                }

                var response = JsonUtility.FromJson<GenericResponse>(request.downloadHandler.text);

                if (!response.IsSuccess)
                {
                    return new SignInResult { IsInvalid = true };
                }

                var token = response.Result;

                Username = username;
                Token = token;

                return new SignInResult { Token = token };
            }
        }

        public async UniTask<bool> ValidateCredentialsAsync(string username, string token)
        {
            var data = JsonUtility.ToJson(new Credentials
            {
                Username = username,
                PasswordOrToken = token
            });

            using (var request = UnityWebRequest.Post(BaseAddress + "User/IsTokenValid", data, JsonContentType))
            {
                await request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    LogFailure(request);
                    return false;
                }

                var response = JsonUtility.FromJson<GenericResponse>(request.downloadHandler.text);

                return response.IsSuccess;
            }
        }

        public async UniTask<bool> SignOutAsync()
        {
            Username = null;
            Token = null;

            using (var request = UnityWebRequest.Get(BaseAddress + "User/SignOut"))
            {
                await request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    LogFailure(request);
                    return false;
                }
            }

            return true;
        }
    }
}
