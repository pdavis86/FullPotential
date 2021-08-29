using MLAPI;
using UnityEngine;

// ReSharper disable CheckNamespace
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedMember.Global

public class MainMenuUi : MonoBehaviour
{
    private MainCanvasObjects _mainCanvasObjects;

    private void Awake()
    {
        _mainCanvasObjects = GameManager.Instance.MainCanvasObjects;
    }

    public void Disconnect()
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
        if (!NetworkManager.Singleton.IsServer)
        {
            return;
        }
        if (GameManager.Instance.DataStore.LocalPlayer != null)
        {
            GameManager.Instance.DataStore.LocalPlayer.GetComponent<PlayerState>().Save();
        }
    }

}
