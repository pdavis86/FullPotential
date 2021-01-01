using Assets.Scripts.Ui.Crafting;
using Assets.Scripts.Ui.Crafting.Items;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
        var newComp = Instantiate(_slotTemplate, Container.transform);
        newComp.gameObject.GetComponent<ComponentProperties>().Properties = new CraftableBase { 
            Attributes = _resultFactory.GetRandomAttributes(), 
            Effects = new List<string> { _resultFactory.GetRandomEffect() }
        };
        newComp.gameObject.SetActive(true);

        UiHelper.UpdateResults(Container.transform.parent.parent, _resultFactory);
    }

}
