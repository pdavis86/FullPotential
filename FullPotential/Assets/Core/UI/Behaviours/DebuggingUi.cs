using FullPotential.Api.Ioc;
using FullPotential.Api.Localization;
using FullPotential.Core.GameManagement;
using FullPotential.Core.Utilities.UtilityBehaviours;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global

namespace FullPotential.Core.Ui.Behaviours
{
    public class DebuggingUi : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField] private Text _pingText;
#pragma warning restore 0649

        private ILocalizer _localizer;

        private string _hostString;
        private string _framePerSecond;
        private GameObject _playerObj;
        private NetworkStats _networkStats;

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            _localizer = DependenciesContext.Dependencies.GetService<ILocalizer>();
        }

        // ReSharper disable once UnusedMember.Local
        private void Start()
        {
            if (!Debug.isDebugBuild)
            {
                Destroy(gameObject);
            }

            _hostString = _localizer.Translate("ui.debugging.host");
            GetNetworkStats();

            GetFps();
            InvokeRepeating(nameof(GetFps), 1, 1);
        }

        // ReSharper disable once UnusedMember.Local
        private void OnGUI()
        {
            var networkStats = GetNetworkStats();
            if (networkStats != null)
            {
                var pingTime = NetworkManager.Singleton.IsServer
                    ? _hostString
                    : (int)(networkStats.LastRtt * 1000) + " ms";

                _pingText.text = pingTime + "\n" + _framePerSecond;
            }
        }

        private NetworkStats GetNetworkStats()
        {
            if (GameManager.Instance.LocalGameDataStore.PlayerGameObject == null)
            {
                return null;
            }

            if (_playerObj != GameManager.Instance.LocalGameDataStore.PlayerGameObject)
            {
                _playerObj = GameManager.Instance.LocalGameDataStore.PlayerGameObject;
                _networkStats = GameManager.Instance.LocalGameDataStore.PlayerGameObject.GetComponent<NetworkStats>();
            }

            return _networkStats;
        }

        private void GetFps()
        {
            _framePerSecond = (int)(1f / Time.smoothDeltaTime) + " FPS";
        }

    }
}
