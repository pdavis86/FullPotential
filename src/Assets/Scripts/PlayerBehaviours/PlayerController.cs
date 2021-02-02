using System;
using UnityEngine;

// ReSharper disable once CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Global
public class PlayerController : MonoBehaviour
{
    public Camera PlayerCamera;

    void Update()
    {
        try
        {
            //todo: make a setting
            if (Input.GetKeyDown(KeyCode.E))
            {
                var startPos = PlayerCamera.transform.position;
                if (Physics.Raycast(startPos, PlayerCamera.transform.forward, out var hit))
                {
                    //Debug.DrawLine(startPos, hit.point, Color.blue, 3);
                    Debug.Log("Ray cast hit " + hit.collider.gameObject.name);

                    var interactable = hit.collider.GetComponent<Interactable>();
                    if (interactable != null)
                    {
                        var distance = Vector3.Distance(PlayerCamera.transform.position, interactable.transform.position);
                        if (distance <= interactable.Radius)
                        {
                            Debug.Log("Interacted with " + hit.collider.gameObject.name);
                            interactable.InteractWith();
                        }
                        else
                        {
                            Debug.Log($"But not close enough ({distance})");
                        }
                    }
                    else
                    {
                        Debug.Log("But it's not interactable");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }

}
