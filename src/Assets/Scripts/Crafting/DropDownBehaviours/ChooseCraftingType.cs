using Assets.Scripts.Crafting;
using Assets.Scripts.Crafting.Results;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable once CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnassignedField.Global
// ReSharper disable MemberCanBePrivate.Global

public class ChooseCraftingType : MonoBehaviour
{
    public const string CraftingTypeWeapon = "Weapon";
    public const string CraftingTypeArmor = "Armor";
    public const string CraftingTypeAccessory = "Accessory";
    public const string CraftingTypeSpell = "Spell";

    public Dropdown SubTypeDropdown;
    public Dropdown HandednessDropdown;

    private Dropdown _thisDropdown;

    public static readonly List<string> TypeOptions = new List<string>
    {
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
    }

}
