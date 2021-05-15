using UnityEngine;
using UnityEngine.UI;

public class Hud : MonoBehaviour
{
    [SerializeField]
    private GameObject _alertsContainer;

    [SerializeField]
    private GameObject _alertPrefab;

    public void ShowAlert(string alertText)
    {
        //Debug.Log($"{addedItems.First().Name} was added");

        var alert = Instantiate(_alertPrefab, _alertsContainer.transform);
        alert.transform.Find("Text").GetComponent<Text>().text = alertText;
    }

}
