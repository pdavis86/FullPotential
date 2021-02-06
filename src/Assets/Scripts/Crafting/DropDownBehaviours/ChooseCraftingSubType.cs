using Assets.Scripts.Crafting;
using Assets.Scripts.Crafting.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable once CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Global

public class ChooseCraftingSubType : MonoBehaviour
{
    public Dropdown TypeDropdown;
    public Dropdown HandednessDropdown;

    public static readonly List<string> HandednessOptions = new List<string>
    {
        Weapon.OneHanded,
        Weapon.TwoHanded
    };

    private Dropdown _subTypeDropdown;

    // ReSharper disable once InconsistentNaming
    private static readonly string[] _handednessSubTypes = new[]
    {
        Weapon.Axe,
        Weapon.Sword,
        Weapon.Hammer,
        Weapon.Gun
    };

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
        try
        {
            SetHandednessDropDownVisibility(HandednessDropdown, TypeDropdown.options[TypeDropdown.value].text, _subTypeDropdown.options[index].text);
            UiHelper.Instance.UpdateResults();
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    public static void SetHandednessDropDownVisibility(Dropdown handednessDropdown, string type, string subType)
    {
        handednessDropdown.gameObject.SetActive(type == ChooseCraftingType.CraftingTypeWeapon && _handednessSubTypes.Contains(subType));
    }

}
