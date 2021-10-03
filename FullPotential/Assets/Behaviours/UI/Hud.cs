using UnityEngine;
using UnityEngine.UI;

// ReSharper disable CheckNamespace
// ReSharper disable ClassNeverInstantiated.Global

public class Hud : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private GameObject _alertsContainer;
    [SerializeField] private GameObject _alertPrefab;
    [SerializeField] private Slider _healthSlider;
    //[SerializeField] private Slider _manaSlider;
#pragma warning restore 0649

    public void ShowAlert(string alertText)
    {
        var alert = Instantiate(_alertPrefab, _alertsContainer.transform);
        alert.transform.Find("Text").GetComponent<Text>().text = alertText;
    }

    public void UpdateHealthPercentage(float value)
    {
        _healthSlider.value = value;
    }

    //todo: this and barrier too
    //public void UpdateManaPercentage(float value)
    //{
    //    _manaSlider.value = value;
    //}

}
