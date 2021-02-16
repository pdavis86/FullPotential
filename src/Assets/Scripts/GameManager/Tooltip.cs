using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour
{
    private static Tooltip _instance;

    private Text _tooltipText;
    private RectTransform _rect;

    private void Awake()
    {
        _instance = this;
        _tooltipText = transform.Find("Text").GetComponent<Text>();
        _rect = GetComponent<RectTransform>();
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (gameObject.activeSelf)
        {
            transform.position = Input.mousePosition;
        }
    }

    public void Show(string tooltipText)
    {
        const int padding = 8;

        gameObject.SetActive(true);

        _tooltipText.text = tooltipText;
        _rect.sizeDelta = new Vector2(_tooltipText.preferredWidth + padding, _tooltipText.preferredHeight + padding);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public static void ShowTooltip(string tooltipText)
    {
        _instance.Show(tooltipText);
    }

    public static void HideTooltip()
    {
        _instance.Hide();
    }

}
