using Assets.Scripts.Crafting.Results;
using System.Linq;
using UnityEngine;

// ReSharper disable once CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

public class GameManager : MonoBehaviour
{
    public Inventory Inventory { get; private set; }

    public InputMappings InputMappings { get; private set; }

    public ResultFactory ResultFactory { get; private set; }


    // ReSharper disable once ArrangeAccessorOwnerBody
    private static GameManager _instance;
    public static GameManager Instance { get { return _instance; } }


    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
            Inventory = GetComponent<Inventory>();
            InputMappings = GetComponent<InputMappings>();
            ResultFactory = new ResultFactory();

            DontDestroyOnLoad(gameObject);
        }
    }

    public static GameObject GetSceneObjects()
    {
        return UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects().FirstOrDefault(x => x.name == "SceneObjects");
    }

    public static GameObject GetCurrentPlayerGameObject(Camera playerCamera)
    {
        //var players = GameObject.FindGameObjectsWithTag("Player");
        return playerCamera.gameObject.transform.parent.gameObject;
    }

}
