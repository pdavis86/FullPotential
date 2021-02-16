using Assets.Scripts.Crafting;
using Assets.Scripts.Crafting.Results;
using System;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable once CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global

public class ComponentProperties : MonoBehaviour
{
    public ItemBase Properties;

    void Start()
    {
        transform.Find("Slot").Find("RemoveButton") .GetComponent<Button>().onClick.AddListener(OnRemoveClick);
        //todo: ? transform.Find("Slot").Find("SlotButton").GetComponent<Button>().onClick.AddListener(OnRemoveClick);
    }

    public void OnShowTooltip()
    {
        Tooltip.ShowTooltip("This is my tooltip\n for component");
    }


    private void OnRemoveClick()
    {
        try
        {
            //Change the name so it is not used in the UI before being destroyed
            gameObject.name = "Deleting";

            Destroy(gameObject);

            UiHelper.Instance.UpdateResults();
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }
}
