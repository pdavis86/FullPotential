using Assets.Scripts.Ui.Crafting;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChooseCraftingType : MonoBehaviour
{
    public const string Weapon = "Weapon";
    public const string Armor = "Armor";
    public const string Accessory = "Accessory";
    public const string Spell = "Spell";

    public Dropdown SubTypeDropdown;
    public Dropdown HandednessDropdown;

    private Dropdown _thisDropdown;

    public static List<string> TypeOptions = new List<string> {
        Weapon,
        Armor,
        Accessory,
        Spell
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
            case Weapon: SubTypeDropdown.AddOptions(Assets.Scripts.Ui.Crafting.Items.Weapon.WeaponOptions); break;
            case Armor: SubTypeDropdown.AddOptions(Assets.Scripts.Ui.Crafting.Items.Armor.ArmorOptions); break;
            case Accessory: SubTypeDropdown.AddOptions(Assets.Scripts.Ui.Crafting.Items.Accessory.AccessoryOptions); break;
            case Spell: isSpell = true; break;

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
