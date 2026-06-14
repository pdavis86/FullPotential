using Cysharp.Threading.Tasks;

using FullPotential.Api.Data;
using FullPotential.Api.GameManagement.Models;
using FullPotential.Api.Utilities.Extensions;
using FullPotential.Models.User;
using FullPotential.Models.Utilities;

using Newtonsoft.Json.Linq;

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

                var response = request.downloadHandler.text.FromJson<GenericResponse>();

                if (!response.IsSuccess)
                {
                    return new SignInResult { IsInvalid = true };
                }

                var userData = ((JObject)response.Result).ToObject<UserData>();

                Username = username;
                Token = userData.Token;

                return new SignInResult
                {
                    UserId = userData.UserId,
                    Username = userData.Username,
                    Token = userData.Token,
                    CharacterId = userData.CharacterId
                };
            }
        }

        public async UniTask<SignInResult> SignInWithTokenAsync(string username, string token)
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
                    return new SignInResult();
                }

                var response = request.downloadHandler.text.FromJson<GenericResponse>();

                if (!response.IsSuccess)
                {
                    return new SignInResult { IsInvalid = true };
                }

                var userData = ((JObject)response.Result).ToObject<UserData>();

                Username = username;
                Token = userData.Token;

                return new SignInResult
                {
                    UserId = userData.UserId,
                    Username = userData.Username,
                    Token = userData.Token,
                    CharacterId = userData.CharacterId
                };
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
