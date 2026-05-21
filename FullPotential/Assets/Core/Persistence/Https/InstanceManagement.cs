namespace FullPotential.Core.Persistence.Https
{
    using Cysharp.Threading.Tasks;

    using FullPotential.Api.Data;
    using FullPotential.Api.GameManagement.Models;

    using UnityEngine;
    using UnityEngine.Networking;

    // ReSharper disable ClassNeverInstantiated.Global

    public class InstanceManagement : HttpsPersistenceBase, IInstanceManagement
    {
        public InstanceManagement(ISettingsRepository settingsRepository)
            : base(settingsRepository)
        {
        }

        public async UniTask<ConnectionDetails> GetConnectionDetailsAsync()
        {
            using (var request = UnityWebRequest.Get(BaseAddress + "Instance/GetConnectionDetails"))
            {
                SetAuthenticationHeader(request);

                await request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    LogFailure(request);
                    return null;
                }

                var result = JsonUtility.FromJson<ConnectionDetails>(request.downloadHandler.text);
                return result;
            }
        }

        public UniTask SaveConnectionDetailsAsync(ConnectionDetails connectionDetails)
        {
            throw new System.NotImplementedException();
        }
    }
}
