using UnityEngine;

public class MainCanvasObjects : MonoBehaviour
{
    public GameObject HitNumberContainer;
    public GameObject Hud;
    public GameObject CraftingUi;
    public GameObject DebuggingOverlay;

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
    }
}
