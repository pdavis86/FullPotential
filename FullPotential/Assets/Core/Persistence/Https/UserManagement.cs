using System;
using System.Collections;

using FullPotential.Api.Data;
using FullPotential.Api.GameManagement.JsonModels;
using FullPotential.Api.Obsolete;

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
            // todo: check there is a token
            return Token;
        }

        public IEnumerator SignInWithPasswordEnumerator(string username, string password, Action<string> successCallback, Action<bool> failureCallback)
        {
            var data = JsonUtility.ToJson(new Credentials
            {
                Username = username,
                PasswordOrToken = password
            });

            using (var request = UnityWebRequest.Post(BaseAddress + "User/SignInWithPassword", data, JsonContentType))
            {
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    LogFailure(request);
                    failureCallback(false);
                    yield break;
                }

                var response = JsonUtility.FromJson<GenericResponse>(request.downloadHandler.text);

                if (!response.IsSuccess)
                {
                    failureCallback(true);
                    yield break;
                }

                var token = response.Result;

                Username = username;
                Token = token;

                successCallback(token);
            }
        }

        public IEnumerator SignOutEnumerator(Action successCallback, Action failureCallback)
        {
            Username = null;
            Token = null;

            using (var request = UnityWebRequest.Get(BaseAddress + "User/SignOut"))
            {
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    LogFailure(request);
                    failureCallback();
                    yield break;
                }

                successCallback();
            }
        }

        public IEnumerator ValidateCredentialsEnumerator(string username, string token, Action successCallback, Action failureCallback)
        {
            var data = JsonUtility.ToJson(new Credentials
            {
                Username = username,
                PasswordOrToken = token
            });

            using (var request = UnityWebRequest.Post(BaseAddress + "User/IsTokenValid", data, JsonContentType))
            {
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    LogFailure(request);
                    failureCallback();
                    yield break;
                }

                var response = JsonUtility.FromJson<GenericResponse>(request.downloadHandler.text);

                if (response.IsSuccess)
                {
                    successCallback();
                }
                else
                {
                    failureCallback();
                }
            }
        }
    }
}
