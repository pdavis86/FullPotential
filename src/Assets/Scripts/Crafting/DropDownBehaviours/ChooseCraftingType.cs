﻿using Assets.Scripts.Crafting;
using Assets.Scripts.Crafting.Results;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChooseCraftingType : MonoBehaviour
{
    public const string CraftingTypeWeapon = "Weapon";
    public const string CraftingTypeArmor = "Armor";
    public const string CraftingTypeAccessory = "Accessory";
    public const string CraftingTypeSpell = "Spell";

    public Dropdown SubTypeDropdown;
    public Dropdown HandednessDropdown;

    private Dropdown _thisDropdown;

    public static List<string> TypeOptions = new List<string> {
        CraftingTypeWeapon,
        CraftingTypeArmor,
        CraftingTypeAccessory,
        CraftingTypeSpell
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
        var craftingType = _thisDropdown.options[_thisDropdown.value].text;

        switch (craftingType)
        {
            case CraftingTypeWeapon: SubTypeDropdown.AddOptions(Weapon.WeaponOptions); break;
            case CraftingTypeArmor: SubTypeDropdown.AddOptions(Armor.ArmorOptions); break;
            case CraftingTypeAccessory: SubTypeDropdown.AddOptions(Accessory.AccessoryOptions); break;
            case CraftingTypeSpell: isSpell = true; break;

            default:
                throw new InvalidOperationException("Unknown crafting type");
        }

        if (isSpell)
        {
            SubTypeDropdown.gameObject.SetActive(false);
        }
        else
        {
            SubTypeDropdown.RefreshShownValue();
            SubTypeDropdown.gameObject.SetActive(true);
        }

        var subType = SubTypeDropdown.options != null ? SubTypeDropdown.options[SubTypeDropdown.value].text : null;
        ChooseCraftingSubType.SetHandednessDropDownVisibility(HandednessDropdown, craftingType, subType);

        UiHelper.UpdateResults(transform.parent.parent, new ResultFactory());
    }

}