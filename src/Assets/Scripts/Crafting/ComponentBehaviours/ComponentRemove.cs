using Assets.Scripts.Crafting;
using System;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable once CheckNamespace
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global

public class ComponentRemove : MonoBehaviour
{
    void Start()
    {
        transform.GetComponent<Button>().onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        try
        {
            //Change the name so it is not used in the UI before being destroyed
            var slot = transform.parent.parent.gameObject;
            slot.name = "Deleting";

            Destroy(slot);

            UiHelper.Instance.UpdateResults();
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }

}
