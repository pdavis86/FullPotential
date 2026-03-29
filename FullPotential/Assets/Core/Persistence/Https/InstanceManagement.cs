namespace FullPotential.Core.Persistence.Https
{
    using System;
    using System.Collections;

    using FullPotential.Api.Data;
    using FullPotential.Api.GameManagement.JsonModels;

    using UnityEngine;
    using UnityEngine.Networking;

    // ReSharper disable ClassNeverInstantiated.Global

    public class InstanceManagement : HttpsPersistenceBase, IInstanceManagement
    {
        public InstanceManagement(ISettingsRepository settingsRepository)
            : base(settingsRepository)
        {
        }

        public IEnumerator ConnectionDetailsEnumerator(Action<ConnectionDetails> successCallback, Action failureCallback)
        {
            using (var request = UnityWebRequest.Get(BaseAddress + "Instance/GetConnectionDetails"))
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
    }
}
