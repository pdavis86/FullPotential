namespace FullPotential.Core.GameManagement
{
    using System;
    using System.Collections;
    using System.Net.Http;
    using System.Threading.Tasks;
    using FullPotential.Api.GameManagement;
    using FullPotential.Api.GameManagement.JsonModels;
    using Newtonsoft.Json;
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

        public ManagementService()
        {
            //todo: read from settings
            _baseAddress = "https://localhost:7180/";
        }

        public string SignInWithExistingToken()
        {
            var username = PlayerPrefs.GetString(StorageKeyUsername);
            var token = PlayerPrefs.GetString(StorageKeyToken);

            _authHeaderValue = $"{username};{token}";

            return token;
        }

        public IEnumerator SignInWithPasswordCoroutine(string username, string password, Action<string> successCallback, Action<bool> failureCallback)
        {
            var data = JsonConvert.SerializeObject(new
            {
                UserName = username,
                Password = password
            });

            using (var request = UnityWebRequest.Post(_baseAddress + "User/SignInWithPassword", data, JsonContentType))
            {
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    LogFailure(request);
                    failureCallback(request.responseCode == 400);
                    yield break;
                }

                var token = request.downloadHandler.text;

                PlayerPrefs.SetString(StorageKeyUsername, username);
                PlayerPrefs.SetString(StorageKeyToken, token);

                _authHeaderValue = $"{username};{token}";

                successCallback(token);
            }
        }

        public IEnumerator ConnectionDetailsCoroutine(Action<ConnectionDetails> successCallback, Action failureCallback)
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

        public IEnumerator SignOutCoroutine(Action successCallback, Action failureCallback)
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

        public async Task<bool> ValidateCredentials(string username, string token)
        {
            //todo: implement
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(_baseAddress + "User/GetUsernameFromToken?token=" + Uri.EscapeDataString(token));
                var temp = await response.Content.ReadAsStringAsync();
                return true;
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
