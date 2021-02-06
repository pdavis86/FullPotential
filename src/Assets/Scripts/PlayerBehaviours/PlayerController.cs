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
    public bool HasMenuOpen;

    private bool _doUiToggle;

    void Awake()
    {
        ObjectAccess.Instance.UiCrafting.SetActive(false);
    }

    void Update()
    {
        try
        {
            //todo: make settings for keys
            if (Input.GetKeyDown(KeyCode.E)) { InteractWith(); }
            if (Input.GetKeyDown(KeyCode.Escape)) { _doUiToggle = true; }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    private void FixedUpdate()
    {
        try
        {
            if (_doUiToggle)
            {
                _doUiToggle = false;

                ObjectAccess.Instance.UiHud.SetActive(!ObjectAccess.Instance.UiHud.activeSelf);
                ObjectAccess.Instance.UiCrafting.SetActive(!ObjectAccess.Instance.UiHud.activeSelf);

                HasMenuOpen = !ObjectAccess.Instance.UiHud.activeSelf;

                if (HasMenuOpen)
                {
                    if (Cursor.lockState != CursorLockMode.None)
                    {
                        Cursor.lockState = CursorLockMode.None;
                    }
                }
                else if (Cursor.lockState != CursorLockMode.Locked)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    void InteractWith()
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

    private void OnDisable()
    {
        if (ObjectAccess.Instance.UiHud != null)
        {
            ObjectAccess.Instance.UiHud.SetActive(false);
            ObjectAccess.Instance.UiCrafting.SetActive(false);
        }
    }




    //public static bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaysastResults)
    //{
    //    var uiLayer = LayerMask.NameToLayer("UI");
    //    return GetEventSystemRaycastResults().FirstOrDefault(x => x.gameObject.layer == uiLayer).gameObject != null;
    //}

    //static List<RaycastResult> GetEventSystemRaycastResults()
    //{
    //    var raycastResults = new List<RaycastResult>();
    //    EventSystem.current.RaycastAll(
    //        new PointerEventData(EventSystem.current)
    //        {
    //            position = Input.mousePosition
    //        }, raycastResults
    //    );
    //    return raycastResults;
    //}

}
