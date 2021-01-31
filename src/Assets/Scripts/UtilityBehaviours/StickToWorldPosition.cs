using UnityEngine;

// ReSharper disable once CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global

public class StickToWorldPosition : MonoBehaviour
{
    public Camera PlayerCamera;
    public Vector3 WorldPosition;

    void Update()
    {
        transform.position = PlayerCamera.WorldToScreenPoint(WorldPosition);
    }
}
