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
        GameManager.Instance.GameObjects.UiCrafting.SetActive(false);
    }

    void Update()
    {
        try
        {
            var mappings = GameManager.Instance.InputMappings;
            if (Input.GetKeyDown(mappings.Menu)) { _doUiToggle = true; }
            else if (Input.GetKeyDown(mappings.Inventory)) { OpenInventory(); }
            else if (Input.GetKeyDown(mappings.Interact)) { InteractWith(); }
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

                GameManager.Instance.GameObjects.UiHud.SetActive(!GameManager.Instance.GameObjects.UiHud.activeSelf);
                GameManager.Instance.GameObjects.UiCrafting.SetActive(!GameManager.Instance.GameObjects.UiHud.activeSelf);

                HasMenuOpen = !GameManager.Instance.GameObjects.UiHud.activeSelf;
            }

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
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance.GameObjects.UiHud != null)
        {
            GameManager.Instance.GameObjects.UiHud.SetActive(false);
            GameManager.Instance.GameObjects.UiCrafting.SetActive(false);
        }
    }




    void InteractWith()
    {
        var startPos = PlayerCamera.transform.position;
        if (Physics.Raycast(startPos, PlayerCamera.transform.forward, out var hit))
        {
            //Debug.DrawLine(startPos, hit.point, Color.blue, 3);
            //Debug.Log("Ray cast hit " + hit.collider.gameObject.name);

            var interactable = hit.collider.GetComponent<Interactable>();
            if (interactable != null)
            {
                var distance = Vector3.Distance(PlayerCamera.transform.position, interactable.transform.position);
                if (distance <= interactable.Radius)
                {
                    //Debug.Log("Interacted with " + hit.collider.gameObject.name);
                    interactable.InteractWith();
                }
                //else
                //{
                //    Debug.Log($"But not close enough ({distance})");
                //}
            }
            //else
            //{
            //    Debug.Log("But it's not interactable");
            //}
        }
    }

    void OpenInventory()
    {
        //todo: finish this
        Debug.Log("Tried to open inventory");
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
