namespace FullPotential.Core.GameManagement
{
    using System;
    using System.Collections;
    using FullPotential.Api.GameManagement;
    using FullPotential.Api.GameManagement.JsonModels;
    using Newtonsoft.Json;
    using UnityEngine;
    using UnityEngine.Networking;

    // ReSharper disable ClassNeverInstantiated.Global

    public class ManagementService : IManagementService
    {
        private const string JsonContentType = "application/json";

        private readonly string _baseAddress;
        private string _authHeaderValue;

        public ManagementService()
        {
            //todo: read from settings
            _baseAddress = "https://localhost:7180/";
        }

        private void SetCredentials(string username, string token)
        {
            _authHeaderValue = $"{username};{token}";

            //todo: remove
            _authHeaderValue = "a;JwbZmIYhIgelVet0gr8u6Ecq6Rni9MgV";
        }

        public IEnumerator SignInWithPasswordEnumerator(string userName, string password, Action<string> successCallback, Action failureCallback)
        {
            var data = JsonConvert.SerializeObject(new
            {
                UserName = userName,
                Password = password
            });

            using (var request = UnityWebRequest.Post(_baseAddress + "User/SignInWithPassword", data, JsonContentType))
            {
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    LogFailure(request);
                    failureCallback();
                    yield break;
                }

                SetCredentials(userName, request.downloadHandler.text);
                successCallback(request.downloadHandler.text);
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
