using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Compiler

public class Tooltips : MonoBehaviour
{
    private static Tooltips _instance;

    private Text _tooltipText;
    private RectTransform _rect;
    private Vector3 _underOffset;
    private Vector3 _leftOffset;

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
            var pointerPosition = Mouse.current.position.ReadValue();

            var underPointer = (pointerPosition.y + _rect.sizeDelta.y > Screen.height) && _rect.sizeDelta.y < Screen.height;
            var leftOfPointer = (pointerPosition.x + _rect.sizeDelta.x > Screen.width) && _rect.sizeDelta.x < Screen.width;

            transform.position = new Vector3(pointerPosition.x, pointerPosition.y) +
                (underPointer ? _underOffset : new Vector3(0, 1))
                + (leftOfPointer ? _leftOffset : new Vector3(1, 0));
        }
    }

    public void Show(string tooltipText)
    {
        const int padding = 15;

        gameObject.SetActive(true);
        _tooltipText.text = tooltipText;
        _rect.sizeDelta = new Vector2(_tooltipText.preferredWidth + padding, _tooltipText.preferredHeight - 5);

        _underOffset = new Vector3(1, -_rect.sizeDelta.y - 1);
        _leftOffset = new Vector3(-_rect.sizeDelta.x - 1, 1);

        //Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, Camera.main.transform.position.z);
        //Debug.Log("screenCenter " + screenCenter);

        //Vector3 screenHeight = new Vector3(Screen.width / 2, Screen.height, Camera.main.transform.position.z);
        //Debug.Log("screenHeight " + screenHeight);

        //Vector3 screenWidth = new Vector3(Screen.width, Screen.height / 2, Camera.main.transform.position.z);
        //Debug.Log("screenWidth " + screenWidth);

        //Vector3 goscreen = Camera.main.WorldToScreenPoint(transform.position);
        //Debug.Log("GoPos " + goscreen);

        //float distX = Vector3.Distance(new Vector3(Screen.width / 2, 0f, 0f), new Vector3(goscreen.x, 0f, 0f));
        //Debug.Log("distX " + distX);

        //float distY = Vector3.Distance(new Vector3(0f, Screen.height / 2, 0f), new Vector3(0f, goscreen.y, 0f));
        //Debug.Log("distY " + distY);

        //Debug.Log("Y delta " + _rect.sizeDelta.y);
        //Debug.Log("Y mouse " + Input.mousePosition.y);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public static void ShowTooltip(string tooltipText)
    {
        if (!string.IsNullOrWhiteSpace(tooltipText))
        {
            _instance.Show(tooltipText);
        }
    }

    public static void HideTooltip()
    {
        _instance.Hide();
    }

}
