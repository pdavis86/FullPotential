using Assets.Scripts.Crafting.Results;
using System;
using System.Collections;
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

    private Inventory _inventory;
    private List<ItemBase> _components;

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
        LoadInventory();

        _typeDropdown.ClearOptions();
        _typeDropdown.AddOptions(TypeOptions);

        _handednessDropdown.ClearOptions();
        _handednessDropdown.AddOptions(HandednessOptions);

        TypeOnValueChanged(0);
        _handednessDropdown.gameObject.SetActive(false);
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

    public void LoadInventory()
    {
        _componentsContainer.transform.Clear();

        var rowRectTransform = _rowPrefab.GetComponent<RectTransform>();
        var rowCounter = 0;

        //todo: only appropriate items for crafting type
        foreach (var item in _inventory.Items)
        {
            var row = Instantiate(_rowPrefab, _componentsContainer.transform);
            //row.GetComponent<RectTransform>().position = rowRectTransform.position + new Vector3(0, rowCounter * rowRectTransform.rect.height);
            //row.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -rowHeight * i);

            if (item.Name == "Scrap" || item.Name == "Shard")
            {
                var suffix = int.Parse(item.GetHashCode().ToString().TrimStart('-').Substring(5));
                row.transform.Find("ItemName").GetComponent<Text>().text = item.Name + $" (Type #{suffix.ToString("D5")})";
            }
            else
            {
                row.transform.Find("ItemName").GetComponent<Text>().text = item.Name;
            }

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
                Tooltip.ShowTooltip(GetItemDescription(item, false));
            };

            rowCounter++;
        }

        var containerRectTrans = _componentsContainer.GetComponent<RectTransform>();
        containerRectTrans.sizeDelta = new Vector2(containerRectTrans.sizeDelta.x, rowCounter * rowRectTransform.rect.height);
    }

    private void Tooltip_onPointerEnter(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }

    private void UpdateResults()
    {
        if (_components.Count == 0)
        {
            _outputText.text = null;
            return;
        }

        var selectedType = _typeDropdown.options[_typeDropdown.value].text;
        var selectedSubtype = _subTypeDropdown.options.Count > 0 ? _subTypeDropdown.options[_subTypeDropdown.value].text : null;
        var isTwoHanded = _handednessDropdown.options.Count > 0 && _handednessDropdown.options[_handednessDropdown.value].text == Weapon.TwoHanded;
        var craftedThing = CmdGetCraftedItem(_components, selectedType, selectedSubtype, isTwoHanded);

        _outputText.text = GetItemDescription(craftedThing);
    }

    public static string GetItemDescription(ItemBase item, bool includeName = true)
    {
        var sb = new StringBuilder();

        if (includeName) { sb.Append($"Name: {item.Name}\n"); }
        if (item.Attributes.IsActivated) { sb.Append("IsActivated: true\n"); }
        if (item.Attributes.IsAutomatic) { sb.Append("IsAutomatic: true\n"); }
        if (item.Attributes.IsSoulbound) { sb.Append("IsSoulbound: true\n"); }
        if (item.Attributes.ExtraAmmoPerShot > 0) { sb.Append($"ExtraAmmoPerShot: {item.Attributes.ExtraAmmoPerShot}\n"); }
        if (item.Attributes.Strength > 0) { sb.Append($"Strength: {item.Attributes.Strength}\n"); }
        if (item.Attributes.Cost > 0) { sb.Append($"Cost: {item.Attributes.Cost}\n"); }
        if (item.Attributes.Range > 0) { sb.Append($"Range: {item.Attributes.Range}\n"); }
        if (item.Attributes.Accuracy > 0) { sb.Append($"Accuracy: {item.Attributes.Accuracy}\n"); }
        if (item.Attributes.Speed > 0) { sb.Append($"Speed: {item.Attributes.Speed}\n"); }
        if (item.Attributes.Recovery > 0) { sb.Append($"Recovery: {item.Attributes.Recovery}\n"); }
        if (item.Attributes.Duration > 0) { sb.Append($"Duration: {item.Attributes.Duration}\n"); }
        if (item.Effects.Count > 0) { sb.Append($"Effects: {string.Join(", ", item.Effects)}"); }

        return sb.ToString();
    }

    //todo: this needs moving to a NetworkBehaviour to use - [command]
    private ItemBase CmdGetCraftedItem(List<ItemBase> components, string selectedType, string selectedSubtype, bool isTwoHanded)
    {
        //todo: check the components are actually in the player's invesntory
        //todo: requirements e.g. strength, speed, accuracy, 6 scrap or less

        var resultFactory = GameManager.Instance.ResultFactory;

        ItemBase craftedThing;
        if (selectedType == CraftingTypeSpell)
        {
            craftedThing = resultFactory.GetSpell(components);
        }
        else
        {
            switch (selectedSubtype)
            {
                case Weapon.Dagger: craftedThing = resultFactory.GetMeleeWeapon(Weapon.Dagger, components, false); break;
                case Weapon.Spear: craftedThing = resultFactory.GetMeleeWeapon(Weapon.Spear, components, true); break;
                case Weapon.Bow: craftedThing = resultFactory.GetRangedWeapon(Weapon.Bow, components, true); break;
                case Weapon.Crossbow: craftedThing = resultFactory.GetRangedWeapon(Weapon.Crossbow, components, true); break;
                case Weapon.Shield: craftedThing = resultFactory.GetShield(components); break;

                case Armor.Helm: craftedThing = resultFactory.GetArmor(Armor.Helm, components); break;
                case Armor.Chest: craftedThing = resultFactory.GetArmor(Armor.Chest, components); break;
                case Armor.Legs: craftedThing = resultFactory.GetArmor(Armor.Legs, components); break;
                case Armor.Feet: craftedThing = resultFactory.GetArmor(Armor.Feet, components); break;
                case Armor.Gloves: craftedThing = resultFactory.GetArmor(Armor.Gloves, components); break;
                case Armor.Barrier: craftedThing = resultFactory.GetBarrier(components); break;

                case Accessory.Amulet: craftedThing = resultFactory.GetAccessory(Accessory.Amulet, components); break;
                case Accessory.Ring: craftedThing = resultFactory.GetAccessory(Accessory.Ring, components); break;
                case Accessory.Belt: craftedThing = resultFactory.GetAccessory(Accessory.Belt, components); break;

                default:

                    switch (selectedSubtype)
                    {
                        case Weapon.Axe: craftedThing = resultFactory.GetMeleeWeapon(Weapon.Axe, components, isTwoHanded); break;
                        case Weapon.Sword: craftedThing = resultFactory.GetMeleeWeapon(Weapon.Sword, components, isTwoHanded); break;
                        case Weapon.Hammer: craftedThing = resultFactory.GetMeleeWeapon(Weapon.Hammer, components, isTwoHanded); break;
                        case Weapon.Gun: craftedThing = resultFactory.GetRangedWeapon(Weapon.Gun, components, isTwoHanded); break;
                        default:
                            throw new System.Exception("Invalid weapon type");
                    }
                    break;
            }
        }

        return craftedThing;
    }



}
