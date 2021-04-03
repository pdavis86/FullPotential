using Assets.Core.Crafting;
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
    [SerializeField] private GameObject _componentsContainer;
    [SerializeField] private GameObject _rowPrefab;
    [SerializeField] private Text _outputText;
    [SerializeField] private Dropdown _typeDropdown;
    [SerializeField] private Dropdown _subTypeDropdown;
    [SerializeField] private Dropdown _handednessDropdown;
    [SerializeField] private Button _craftButton;

    private Inventory _inventory;
    private List<ItemBase> _components;

    public static readonly List<string> TypeOptions = new List<string>
    {
        ResultFactory.CraftingTypeWeapon,
        ResultFactory.CraftingTypeArmor,
        ResultFactory.CraftingTypeAccessory,
        ResultFactory.CraftingTypeSpell
    };

    public static readonly List<string> HandednessOptions = new List<string>
    {
        Weapon.OneHanded,
        Weapon.TwoHanded
    };

    public static readonly string[] HandednessSubTypes = new[]
    {
        Weapon.Axe,
        Weapon.Sword,
        Weapon.Hammer,
        Weapon.Gun
    };

    private void Awake()
    {
        _components = new List<ItemBase>();

        _inventory = GameManager.Instance.LocalPlayer.GetComponent<Inventory>();

        _typeDropdown.onValueChanged.AddListener(TypeOnValueChanged);

        _subTypeDropdown.onValueChanged.AddListener(SubTypeOnValueChanged);

        _craftButton.onClick.AddListener(CraftButtonOnClick);
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

    public static void SetHandednessDropDownVisibility(Dropdown handednessDropdown, string type, string subType)
    {
        handednessDropdown.gameObject.SetActive(type == ResultFactory.CraftingTypeWeapon && HandednessSubTypes.Contains(subType));
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
                case ResultFactory.CraftingTypeWeapon: _subTypeDropdown.AddOptions(Weapon.WeaponOptions); break;
                case ResultFactory.CraftingTypeArmor: _subTypeDropdown.AddOptions(Armor.ArmorOptions); break;
                case ResultFactory.CraftingTypeAccessory: _subTypeDropdown.AddOptions(Accessory.AccessoryOptions); break;
                case ResultFactory.CraftingTypeSpell: isSpell = true; break;

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
        _typeDropdown.AddOptions(TypeOptions);

        _handednessDropdown.ClearOptions();
        _handednessDropdown.AddOptions(HandednessOptions);

        TypeOnValueChanged(0);
        _handednessDropdown.gameObject.SetActive(false);

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
        return _handednessDropdown.options.Count > 0 && _handednessDropdown.options[_handednessDropdown.value].text == Weapon.TwoHanded;
    }

    private void UpdateResults()
    {
        if (_components.Count == 0)
        {
            _outputText.text = null;
            return;
        }

        var craftedItem = GameManager.Instance.ResultFactory.GetCraftedItem(_components, GetSelectedType(), GetSelectedSubType(), GetSelectedHandedness());
        _outputText.text = ResultFactory.GetItemDescription(craftedItem);
    }

}
