using UnityEngine;
using UnityEngine.UI;

// ReSharper disable CheckNamespace
// ReSharper disable ClassNeverInstantiated.Global

public class Hud : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private GameObject _alertsContainer;
    [SerializeField] private GameObject _alertPrefab;
#pragma warning restore 0649

    public void ShowAlert(string alertText)
    {
        var alert = Instantiate(_alertPrefab, _alertsContainer.transform);
        alert.transform.Find("Text").GetComponent<Text>().text = alertText;
    }

}
