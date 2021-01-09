using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickToWorldPosition : MonoBehaviour
{
    public Camera PlayerCamera;
    public Vector3 WorldPosition;

    void Update()
    {
        transform.position = PlayerCamera.WorldToScreenPoint(WorldPosition);
    }
}
