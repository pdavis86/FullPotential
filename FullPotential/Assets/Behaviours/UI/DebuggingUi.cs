using MLAPI;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Compiler
// ReSharper disable ConvertToNullCoalescingCompoundAssignment

public class DebuggingUi : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private Text _pingText;
#pragma warning restore 0649

    private string _hostString;
    private NetworkStats _networkStats;

    private void Start()
    {
        if (!GameManager.Instance.DataStore.IsDebugging)
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        _hostString = GameManager.Instance.Localizer.Translate("ui.debugging.host");
    }

    void OnGUI()
    {
        var networkStats = GetNetworkStats();
        if (networkStats != null)
        {
            _pingText.text = NetworkManager.Singleton.IsServer
                ? _hostString
                : networkStats.LastRtt + " ms";
        }
    }

    private NetworkStats GetNetworkStats()
    {
        if (GameManager.Instance.DataStore.LocalPlayer == null)
        {
            return null;
        }

        return _networkStats ?? (_networkStats = GameManager.Instance.DataStore.LocalPlayer.GetComponent<NetworkStats>());
    }

}
