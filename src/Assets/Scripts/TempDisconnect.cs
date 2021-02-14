using System;
using UnityEngine;
using UnityEngine.UI;

public class TempDisconnect : MonoBehaviour
{
    void Start()
    {
        var button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        GameManager.Disconnect();
    }

}
