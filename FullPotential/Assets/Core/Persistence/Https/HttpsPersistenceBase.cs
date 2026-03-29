using FullPotential.Api.Data;

using UnityEngine;
using UnityEngine.Networking;

namespace FullPotential.Core.Persistence.Https
{
    public abstract class HttpsPersistenceBase
    {
        protected const string JsonContentType = "application/json";

        private const string StorageKeyUsername = "username";
        private const string StorageKeyToken = "token";

        private string _authHeaderValue;

        protected string BaseAddress { get; private set;}

        protected string Username
        {
            get => PlayerPrefs.GetString(StorageKeyUsername);
            set => PlayerPrefs.SetString(StorageKeyUsername, value);
        }

        protected string Token
        {
            get => PlayerPrefs.GetString(StorageKeyToken);
            set => PlayerPrefs.SetString(StorageKeyToken, value);
        }

        protected HttpsPersistenceBase(ISettingsRepository settingsRepository)
        {
            BaseAddress = settingsRepository.GetOrLoad().ManagementApiAddress;
        }

        protected void SetAuthenticationHeader(UnityWebRequest request)
        {
            request.SetRequestHeader("X-Auth", $"{Username};{Token}");
        }

        protected void LogFailure(UnityWebRequest request)
        {
            Debug.LogError($"Got response code {request.responseCode} with error '{request.error}':\n{request.downloadHandler.text}");
        }
    }
}
