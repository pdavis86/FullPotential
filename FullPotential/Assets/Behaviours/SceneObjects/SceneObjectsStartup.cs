using FullPotential.Assets.Core.Helpers;
using UnityEngine;

// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global

public class SceneObjectsStartup : MonoBehaviour
{
    void Start()
    {
        UnityHelper.GetObjectAtRoot(GameManager.NameCanvasScene).SetActive(true);
    }

}
