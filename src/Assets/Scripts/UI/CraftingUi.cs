using Assets.ApiScripts.Crafting;
using Assets.Core.Crafting;
using Assets.Core.Crafting.Types;
using Assets.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Compiler

public class CraftingUi : MonoBehaviour
{
    public const string OneHanded = "One-handed";
    public const string TwoHanded = "Two-handed";

    [SerializeField] private GameObject _componentsContainer;
    [SerializeField] private GameObject _rowPrefab;
    [SerializeField] private Text _outputText;
    [SerializeField] private Dropdown _typeDropdown;
    [SerializeField] private Dropdown _subTypeDropdown;
    [SerializeField] private Dropdown _handednessDropdown;
    [SerializeField] private Button _craftButton;

    private Inventory _inventory;
    private List<ItemBase> _components;
    private List<string> _armorTypeNames;
    private List<string> _accessoryTypeNames;
    private List<string> _weaponTypeNames;
    private List<string> _twoHandedWeaponTypeNames;

    private readonly List<string> _craftingCategories = new List<string>
    {
        nameof(Weapon),
        nameof(Armor),
        nameof(Accessory),
        nameof(Spell)
    };

    private readonly List<string> _handednessOptions = new List<string>
    {
        OneHanded,
        TwoHanded
    };

    private void Awake()
    {
        _components = new List<ItemBase>();

        _inventory = GameManager.Instance.LocalPlayer.GetComponent<Inventory>();

        _typeDropdown.onValueChanged.AddListener(TypeOnValueChanged);

        _subTypeDropdown.onValueChanged.AddListener(SubTypeOnValueChanged);

        _craftButton.onClick.AddListener(CraftButtonOnClick);

        _armorTypeNames = ApiRegister.Instance.GetCraftables<IGearArmor>()
            .Select(x => x.TypeName)
            .OrderBy(x => x)
            .ToList();

        _accessoryTypeNames = ApiRegister.Instance.GetCraftables<IGearAccessory>()
            .Select(x => x.TypeName)
            .OrderBy(x => x)
            .ToList();

        var weaponTypes = ApiRegister.Instance.GetCraftables<IGearWeapon>();

        _weaponTypeNames = weaponTypes
            .Select(x => x.TypeName)
            .OrderBy(x => x)
            .ToList();

        _twoHandedWeaponTypeNames = weaponTypes
            .Where(x =>
            {
                var weapon = (IGearWeapon)x;
                return !weapon.EnforceTwoHanded && weapon.AllowTwoHanded;
            })
            .Select(x => x.TypeName)
            .OrderBy(x => x)
            .ToList();
    }

    private void CraftButtonOnClick()
    {
        _craftButton.interactable = false;
        _inventory.CmdCraftItem(_components.Select(x => x.Id).ToArray(), GetSelectedType(), GetSelectedSubType(), GetSelectedHandedness());
    }

    void SubTypeOnValueChanged(int index)
    {
        try
        {
            SetHandednessDropDownVisibility(_handednessDropdown, _typeDropdown.options[_typeDropdown.value].text, _subTypeDropdown.options[index].text);
            UpdateResults();
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    private void SetHandednessDropDownVisibility(Dropdown handednessDropdown, string type, string subType)
    {
        handednessDropdown.gameObject.SetActive(type == nameof(Weapon) && _twoHandedWeaponTypeNames.Contains(subType));
    }

    void TypeOnValueChanged(int index)
    {
        try
        {
            _subTypeDropdown.ClearOptions();

            var isSpell = false;
            var craftingType = _typeDropdown.options[_typeDropdown.value].text;

            switch (craftingType)
            {
                case nameof(Weapon): _subTypeDropdown.AddOptions(_weaponTypeNames); break;
                case nameof(Armor): _subTypeDropdown.AddOptions(_armorTypeNames); break;
                case nameof(Accessory): _subTypeDropdown.AddOptions(_accessoryTypeNames); break;
                case nameof(Spell): isSpell = true; break;

                default:
                    throw new InvalidOperationException("Unknown crafting type");
            }

            if (isSpell)
            {
                _subTypeDropdown.gameObject.SetActive(false);
            }
            else
            {
                _subTypeDropdown.RefreshShownValue();
                _subTypeDropdown.gameObject.SetActive(true);
            }

            var subType = _subTypeDropdown.options != null && _subTypeDropdown.options.Count > 0 ? _subTypeDropdown.options[_subTypeDropdown.value].text : null;
            SetHandednessDropDownVisibility(_handednessDropdown, craftingType, subType);

            UpdateResults();
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    private void OnEnable()
    {
        ResetUi();
        LoadInventory();
    }

    private void OnDisable()
    {
        _componentsContainer.transform.Clear();
    }

    public void AddComponent(string itemId)
    {
        var item = _inventory.Items.FirstOrDefault(x => x.Id == itemId);

        if (item == null)
        {
            Debug.LogWarning("No item found with id " + itemId);
            return;
        }

        _components.Add(item);
    }

    public void RemoveComponent(string itemId)
    {
        var item = _components.FirstOrDefault(x => x.Id == itemId);
        if (item != null)
        {
            _components.Remove(item);
        }
    }

    public void ResetUi()
    {
        _typeDropdown.ClearOptions();
        _typeDropdown.AddOptions(_craftingCategories);

        _handednessDropdown.ClearOptions();
        _handednessDropdown.AddOptions(_handednessOptions);

        TypeOnValueChanged(0);

        _outputText.text = null;
        _craftButton.interactable = true;
    }

    public void LoadInventory()
    {
        _components.Clear();

        InventoryItemsList.LoadInventoryItems(
            null,
            _componentsContainer,
            _rowPrefab,
            _inventory,
            HandleRowToggle,
            null,
            false
        );

        //_componentsContainer.transform.Clear();

        //var rowRectTransform = _rowPrefab.GetComponent<RectTransform>();
        //var rowCounter = 0;

        //foreach (var item in _inventory.Items)
        //{
        //    var row = Instantiate(_rowPrefab, _componentsContainer.transform);
        //    row.transform.Find("ItemName").GetComponent<Text>().text = item.GetFullName();

        //    var rowImage = row.GetComponent<Image>();
        //    var toggle = row.GetComponent<Toggle>();
        //    toggle.onValueChanged.AddListener(isOn =>
        //    {
        //        if (isOn)
        //        {
        //            rowImage.color = Color.green;
        //            AddComponent(item.Id);
        //        }
        //        else
        //        {
        //            rowImage.color = Color.white;
        //            RemoveComponent(item.Id);
        //        }

        //        UpdateResults();
        //    });

        //    var tooltip = row.GetComponent<Tooltip>();
        //    tooltip.OnPointerEnterForTooltip += pointerEventData =>
        //    {
        //        Tooltips.ShowTooltip(ResultFactory.GetItemDescription(item, false));
        //    };

        //    rowCounter++;
        //}

        //const int spacer = 5;
        //var containerRectTrans = _componentsContainer.GetComponent<RectTransform>();
        //containerRectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rowCounter * (rowRectTransform.rect.height + spacer));
    }

    private void HandleRowToggle(GameObject row, GameObject slot, ItemBase item)
    {
        var rowImage = row.GetComponent<Image>();
        var toggle = row.GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(isOn =>
        {
            if (isOn)
            {
                rowImage.color = Color.green;
                AddComponent(item.Id);
            }
            else
            {
                rowImage.color = Color.white;
                RemoveComponent(item.Id);
            }

            UpdateResults();
        });
    }

    private string GetSelectedType()
    {
        return _typeDropdown.options[_typeDropdown.value].text;
    }

    private string GetSelectedSubType()
    {
        return _subTypeDropdown.options.Count > 0 ? _subTypeDropdown.options[_subTypeDropdown.value].text : null;
    }

    private bool GetSelectedHandedness()
    {
        return _handednessDropdown.options.Count > 0 && _handednessDropdown.options[_handednessDropdown.value].text == TwoHanded;
    }

    private void UpdateResults()
    {
        if (_components.Count == 0)
        {
            _outputText.text = null;
            return;
        }

        var craftedItem = GameManager.Instance.ResultFactory.GetCraftedItem(
            GetSelectedType(),
            GetSelectedSubType(),
            GetSelectedHandedness(),
            _components
        );

        _outputText.text = ResultFactory.GetItemDescription(craftedItem);
    }

}
