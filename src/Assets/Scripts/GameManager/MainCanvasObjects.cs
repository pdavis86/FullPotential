using UnityEngine;

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
    public GameObject InventoryUi;

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

        //Needs to get instantiated
        TooltipOverlay.SetActive(true);
        
        DontDestroyOnLoad(gameObject);
    }

    public void HideAllMenus()
    {
        CraftingUi.SetActive(false);
        EscMenu.SetActive(false);
        InventoryUi.SetActive(false);
    }

    public bool IsAnyMenuOpen()
    {
        return CraftingUi.activeSelf
            || EscMenu.activeSelf
            || InventoryUi.activeSelf;
    }

    public void HideOthersOpenThis(GameObject ui)
    {
        HideAllMenus();
        ui.SetActive(true);
    }

    public void BackToMainMenu()
    {
        HideAllMenus();
        GameManager.Disconnect();
    }

    public void QuitGame()
    {
        GameManager.Quit();
    }

}
