using FullPotential.Api.Persistence;

namespace FullPotential.Core.GameManagement
{
    using System;
    using System.Collections;
    using FullPotential.Api.GameManagement;
    using FullPotential.Api.GameManagement.JsonModels;
    using UnityEngine;
    using UnityEngine.Networking;

    // ReSharper disable ClassNeverInstantiated.Global

    public class ManagementService : IManagementService
    {
        private const string JsonContentType = "application/json";
        private const string StorageKeyUsername = "username";
        private const string StorageKeyToken = "token";

        private readonly string _baseAddress;
        private string _authHeaderValue;

        public ManagementService(ISettingsRepository settingsRepository)
        {
            var gameSettings = settingsRepository.GetOrLoad();
            _baseAddress = gameSettings.ManagementApiAddress;
        }

        public string SignInWithExistingToken()
        {
            var username = PlayerPrefs.GetString(StorageKeyUsername);
            var token = PlayerPrefs.GetString(StorageKeyToken);

            _authHeaderValue = $"{username};{token}";

            return token;
        }

        public IEnumerator SignInWithPasswordEnumerator(string username, string password, Action<string> successCallback, Action<bool> failureCallback)
        {
            var data = JsonUtility.ToJson(new Credentials
            {
                Username = username,
                PasswordOrToken = password
            });

            using (var request = UnityWebRequest.Post(_baseAddress + "User/SignInWithPassword", data, JsonContentType))
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

                PlayerPrefs.SetString(StorageKeyUsername, username);
                PlayerPrefs.SetString(StorageKeyToken, token);

                _authHeaderValue = $"{username};{token}";

                successCallback(token);
            }
        }

        public IEnumerator ConnectionDetailsEnumerator(Action<ConnectionDetails> successCallback, Action failureCallback)
        {
            using (var request = UnityWebRequest.Get(_baseAddress + "Instance/GetConnectionDetails"))
            {
                SetAuthenticationHeader(request);

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    LogFailure(request);
                    failureCallback();
                    yield break;
                }

                var result = JsonUtility.FromJson<ConnectionDetails>(request.downloadHandler.text);
                successCallback(result);
            }
        }

        public IEnumerator SignOutEnumerator(Action successCallback, Action failureCallback)
        {
            PlayerPrefs.SetString(StorageKeyUsername, null);
            PlayerPrefs.SetString(StorageKeyToken, null);

            using (var request = UnityWebRequest.Get(_baseAddress + "User/SignOut"))
            {
                SetAuthenticationHeader(request);

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    LogFailure(request);
                    failureCallback();
                    yield break;
                }

                _authHeaderValue = null;

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

            using (var request = UnityWebRequest.Post(_baseAddress + "User/IsTokenValid", data, JsonContentType))
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

        private void SetAuthenticationHeader(UnityWebRequest request)
        {
            request.SetRequestHeader("X-Auth", _authHeaderValue);
        }

        private void LogFailure(UnityWebRequest request)
        {
            Debug.LogError($"Got response code {request.responseCode} with error '{request.error}':\n{request.downloadHandler.text}");
        }
    }
}
