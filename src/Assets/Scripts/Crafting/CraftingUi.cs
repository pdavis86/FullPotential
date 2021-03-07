using Assets.Scripts.Crafting.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// ReSharper disable once CheckNamespace
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

    //todo: move these. used in various places
    public const string CraftingTypeWeapon = "Weapon";
    public const string CraftingTypeArmor = "Armor";
    public const string CraftingTypeAccessory = "Accessory";
    public const string CraftingTypeSpell = "Spell";

    public static readonly List<string> TypeOptions = new List<string>
    {
        CraftingTypeWeapon,
        CraftingTypeArmor,
        CraftingTypeAccessory,
        CraftingTypeSpell
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
        var pc = GameManager.Instance.LocalPlayer.GetComponent<PlayerController>();
        pc.CmdCraftItem(_components.Select(x => x.Id), GetSelectedType(), GetSelectedSubType(), GetSelectedHandedness());
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
        handednessDropdown.gameObject.SetActive(type == CraftingTypeWeapon && HandednessSubTypes.Contains(subType));
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
                case CraftingTypeWeapon: _subTypeDropdown.AddOptions(Weapon.WeaponOptions); break;
                case CraftingTypeArmor: _subTypeDropdown.AddOptions(Armor.ArmorOptions); break;
                case CraftingTypeAccessory: _subTypeDropdown.AddOptions(Accessory.AccessoryOptions); break;
                case CraftingTypeSpell: isSpell = true; break;

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
        _componentsContainer.transform.Clear();

        var rowRectTransform = _rowPrefab.GetComponent<RectTransform>();
        var rowCounter = 0;

        foreach (var item in _inventory.Items)
        {
            var row = Instantiate(_rowPrefab, _componentsContainer.transform);
            row.transform.Find("ItemName").GetComponent<Text>().text = item.GetFullName();

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

            var tooltip = row.GetComponent<CraftingTooltip>();
            tooltip.OnPointerEnterForTooltip += pointerEventData =>
            {
                Tooltips.ShowTooltip(GetItemDescription(item, false));
            };

            rowCounter++;
        }

        const int spacer = 5;
        var containerRectTrans = _componentsContainer.GetComponent<RectTransform>();
        containerRectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rowCounter * (rowRectTransform.rect.height + spacer));
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
        _outputText.text = GetItemDescription(craftedItem);
    }

    //todo: move this as it is used in various places
    public static string GetItemDescription(ItemBase item, bool includeName = true)
    {
        var sb = new StringBuilder();

        if (includeName) { sb.Append($"Name: {item.Name}\n"); }
        if (item.Attributes.IsAutomatic) { sb.Append("Automatic\n"); }
        if (item.Attributes.IsSoulbound) { sb.Append("Soulbound\n"); }
        if (item.Attributes.ExtraAmmoPerShot > 0) { sb.Append($"ExtraAmmoPerShot: {item.Attributes.ExtraAmmoPerShot}\n"); }
        if (item.Attributes.Strength > 0) { sb.Append($"Strength: {item.Attributes.Strength}\n"); }
        if (item.Attributes.Efficiency > 0) { sb.Append($"Efficiency: {item.Attributes.Efficiency}\n"); }
        if (item.Attributes.Range > 0) { sb.Append($"Range: {item.Attributes.Range}\n"); }
        if (item.Attributes.Accuracy > 0) { sb.Append($"Accuracy: {item.Attributes.Accuracy}\n"); }
        if (item.Attributes.Speed > 0) { sb.Append($"Speed: {item.Attributes.Speed}\n"); }
        if (item.Attributes.Recovery > 0) { sb.Append($"Recovery: {item.Attributes.Recovery}\n"); }
        if (item.Attributes.Duration > 0) { sb.Append($"Duration: {item.Attributes.Duration}\n"); }
        if (item.Effects.Count > 0) { sb.Append($"Effects: {string.Join(", ", item.Effects)}\n"); }

        return sb.ToString();
    }

}
