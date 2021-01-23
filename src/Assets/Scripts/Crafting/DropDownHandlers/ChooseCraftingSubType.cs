using Assets.Scripts.Ui.Crafting;
using Assets.Scripts.Ui.Crafting.Items;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ChooseCraftingSubType : MonoBehaviour
{
    public Dropdown TypeDropdown;
    public Dropdown HandednessDropdown;

    public static List<string> HandednessOptions = new List<string> {
        Weapon.OneHanded,
        Weapon.TwoHanded
    };

    private Dropdown _subTypeDropdown;
    private readonly string[] _handednessSubTypes = new[] { Weapon.Axe, Weapon.Sword, Weapon.Hammer, Weapon.Gun };

    void Start()
    {
        _subTypeDropdown = transform.GetComponent<Dropdown>();
        _subTypeDropdown.onValueChanged.AddListener(OnValueChanged);

        HandednessDropdown.ClearOptions();
        HandednessDropdown.AddOptions(HandednessOptions);
        HandednessDropdown.gameObject.SetActive(false);
    }

    void OnValueChanged(int index)
    {
        var subTypetext = _subTypeDropdown.options[index].text;
        HandednessDropdown.gameObject.SetActive(TypeDropdown.value == 0 && _handednessSubTypes.Contains(subTypetext));

        UiHelper.UpdateResults(transform.parent.parent, new ResultFactory());
    }

}
