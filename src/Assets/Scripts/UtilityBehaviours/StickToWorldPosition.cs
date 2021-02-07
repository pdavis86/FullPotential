using UnityEngine;

// ReSharper disable once CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global

[System.Obsolete("Use BillboardBehaviour instead")]
public class StickToWorldPosition : MonoBehaviour
{
    public Vector3 WorldPosition;

    void Update()
    {
        transform.position = Camera.main.WorldToScreenPoint(WorldPosition);
    }
}
