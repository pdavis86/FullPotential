using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ReSharper disable once CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Global

public class SceneObjectsOffline : MonoBehaviour
{
    void Start()
    {
        GameManager.GetObjectAtRoot(GameManager.NameCanvasScene).SetActive(true);
    }
    
}
