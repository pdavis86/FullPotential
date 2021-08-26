using UnityEngine;

// ReSharper disable CheckNamespace
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedMember.Global

public class EscMenuUi : MonoBehaviour
{
    private MainCanvasObjects _mainCanvasObjects;

    private void Awake()
    {
        _mainCanvasObjects = GameManager.Instance.MainCanvasObjects;
    }

    public void BackToMainMenu()
    {
        Save();
        _mainCanvasObjects.HideAllMenus();
        JoinOrHostGame.Disconnect();
        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(1);
    }

    public void QuitGame()
    {
        Save();
        GameManager.Quit();
    }

    private void Save()
    {
        if (GameManager.Instance.DataStore.LocalPlayer != null)
        {
            GameManager.Instance.DataStore.LocalPlayer.GetComponent<PlayerState>().Save();
        }
    }

}
