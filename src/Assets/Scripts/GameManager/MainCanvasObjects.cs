﻿using UnityEngine;

public class MainCanvasObjects : MonoBehaviour
{
    //Overlays
    public GameObject TooltipOverlay;
    public GameObject DebuggingOverlay;
    public GameObject HitNumberContainer;
    public GameObject Hud;

    //Menus
    public GameObject CraftingUi;
    public GameObject EscMenu;
    public GameObject CharacterMenu;

    // ReSharper disable once ArrangeAccessorOwnerBody
    private static MainCanvasObjects _instance;
    public static MainCanvasObjects Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        TooltipOverlay.SetActive(true);
    }

    public void HideAllMenus()
    {
        CraftingUi.SetActive(false);
        EscMenu.SetActive(false);
        CharacterMenu.SetActive(false);
    }

    public bool IsAnyMenuOpen()
    {
        return CraftingUi.activeSelf
            || EscMenu.activeSelf
            || CharacterMenu.activeSelf;
    }

    public void HideOthersOpenThis(GameObject ui)
    {
        HideAllMenus();
        ui.SetActive(true);
    }

    public void BackToMainMenu()
    {
        HideAllMenus();
        JoinOrHostGame.Disconnect();
    }

    public void QuitGame()
    {
        GameManager.Quit();
    }

}