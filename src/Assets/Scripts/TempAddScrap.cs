using Assets.Scripts.Crafting;
using Assets.Scripts.Crafting.Results;
using System;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable once CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global

public class TempAddScrap : MonoBehaviour
{
    public GameObject Container;

    private Transform _slotTemplate;
    private readonly ResultFactory _resultFactory = new ResultFactory();

    void Start()
    {
        _slotTemplate = Container.transform.Find("ComponentTemplate");
        _slotTemplate.gameObject.SetActive(false);
        transform.GetComponent<Button>().onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        try
        {
            var newComp = Instantiate(_slotTemplate, Container.transform);
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
