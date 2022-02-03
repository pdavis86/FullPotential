using FullPotential.Core.Behaviours.GameManagement;
using FullPotential.Core.Behaviours.UtilityBehaviours;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable once UnusedType.Global

namespace FullPotential.Core.Behaviours.Ui
{
    public class DebuggingUi : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField] private Text _pingText;
#pragma warning restore 0649

        private string _hostString;
        private string _framePerSecond;
        private GameObject _playerObj;
        private NetworkStats _networkStats;

        // ReSharper disable once UnusedMember.Local
        private void Start()
        {
            if (!Debug.isDebugBuild)
            {
                Destroy(gameObject);
            }

            _hostString = GameManager.Instance.Localizer.Translate("ui.debugging.host");
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
            if (GameManager.Instance.LocalGameDataStore.GameObject == null)
            {
                return null;
            }

            if (_playerObj != GameManager.Instance.LocalGameDataStore.GameObject)
            {
                _playerObj = GameManager.Instance.LocalGameDataStore.GameObject;
                _networkStats = GameManager.Instance.LocalGameDataStore.GameObject.GetComponent<NetworkStats>();
            }

            return _networkStats;
        }

        private void GetFps()
        {
            _framePerSecond = (int)(1f / Time.smoothDeltaTime) + " FPS";
        }

    }
}
