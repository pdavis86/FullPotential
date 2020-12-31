using Assets.Scripts.Ui.Crafting;
using Assets.Scripts.Ui.Crafting.Items;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChooseCraftingType : MonoBehaviour
{
    public Dropdown SubTypeDropdown;
    public Dropdown HandednessDropdown;

    private Dropdown _thisDropdown;

    public static List<string> TypeOptions = new List<string> {
        ItemBase.Weapon,
        ItemBase.Armor,
        ItemBase.Accessory,
        ItemBase.Spell
    };

    void Start()
    {
        _thisDropdown = transform.GetComponent<Dropdown>();
        _thisDropdown.ClearOptions();
        _thisDropdown.AddOptions(TypeOptions);
        _thisDropdown.onValueChanged.AddListener(OnValueChanged);
        OnValueChanged(0);
    }

    void OnValueChanged(int index)
    {
        SubTypeDropdown.ClearOptions();

        var isSpell = false;

        switch (_thisDropdown.options[_thisDropdown.value].text)
        {
            case ItemBase.Weapon: SubTypeDropdown.AddOptions(Weapon.WeaponOptions); break;
            case ItemBase.Armor: SubTypeDropdown.AddOptions(Armor.ArmorOptions); break;
            case ItemBase.Accessory: SubTypeDropdown.AddOptions(Accessory.AccessoryOptions); break;
            case ItemBase.Spell: isSpell = true; break;

            default:
                throw new InvalidOperationException("Unknown crafting type");
        }

        if (isSpell)
        {
            SubTypeDropdown.gameObject.SetActive(false);
            HandednessDropdown.gameObject.SetActive(false);
        }
        else
        {
            SubTypeDropdown.RefreshShownValue();
            SubTypeDropdown.gameObject.SetActive(true);
        }

        UiHelper.UpdateResults(transform.parent.parent, new ResultFactory());
    }

}
