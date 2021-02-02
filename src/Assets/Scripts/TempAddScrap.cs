using Assets.Scripts.Crafting;
using Assets.Scripts.Crafting.Results;
using System;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable once CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Global

public class TempAddScrap : MonoBehaviour
{
    public GameObject Container;
    public Transform SlotTemplate;

    private readonly ResultFactory _resultFactory = new ResultFactory();

    void Start()
    {
        SlotTemplate.gameObject.SetActive(false);
        transform.GetComponent<Button>().onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        try
        {
            var newComp = Instantiate(SlotTemplate, Container.transform);
            newComp.gameObject.GetComponent<ComponentProperties>().Properties = _resultFactory.GetLootDrop();
            newComp.gameObject.SetActive(true);

            UiHelper.UpdateResults(Container.transform.parent.parent, _resultFactory);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }

}
