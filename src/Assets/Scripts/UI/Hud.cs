using UnityEngine;
using UnityEngine.UI;

public class Hud : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private GameObject _alertsContainer;
    [SerializeField] private GameObject _alertPrefab;
#pragma warning restore 0649

    public void ShowAlert(string alertText)
    {
        //Debug.Log($"{addedItems.First().Name} was added");

        var alert = Instantiate(_alertPrefab, _alertsContainer.transform);
        alert.transform.Find("Text").GetComponent<Text>().text = alertText;
    }

}
