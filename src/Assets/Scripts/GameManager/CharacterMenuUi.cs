using Assets.Scripts.Crafting.Results;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable once CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Compiler

public class CharacterMenuUi : MonoBehaviour
{
    [SerializeField] private GameObject _componentsContainer;
    [SerializeField] private GameObject _rowPrefab;

    private Inventory _inventory;
    private GameObject _activeSlot;

    private void Awake()
    {
        _inventory = GameManager.Instance.LocalPlayer.GetComponent<Inventory>();
    }

    private void OnEnable()
    {
        ResetUi();
    }

    public void ResetUi()
    {
        _componentsContainer.SetActive(false);
    }

    public void OnSlotClick(Object clickedObject)
    {
        switch (clickedObject.name)
        {
            case "LeftHand":
            case "RightHand":
                _activeSlot = clickedObject as GameObject;
                LoadInventoryItems(new[] { typeof(Weapon), typeof(Spell) });
                break;

            default:
                //todo: finish this
                Debug.LogError("Not yet implemented");
                break;
        }
    }

    private void LoadInventoryItems(IEnumerable<System.Type> itemTypes)
    {
        //todo: once this is working make the gameobjects and code code common with crafting ui

        _componentsContainer.SetActive(true);

        _componentsContainer.transform.Clear();

        var rowRectTransform = _rowPrefab.GetComponent<RectTransform>();
        var rowCounter = 0;

        var itemsOfTypes = _inventory.Items.Where(x => itemTypes.Contains(x.GetType()));

        if (itemsOfTypes.Count() == 0)
        {
            Debug.LogWarning("There are no items of the correct type");
        }

        foreach (var item in itemsOfTypes)
        {
            var row = Instantiate(_rowPrefab, _componentsContainer.transform);
            row.transform.Find("ItemName").GetComponent<Text>().text = item.GetFullName();

            var rowImage = row.GetComponent<Image>();
            var toggle = row.GetComponent<Toggle>();
            toggle.onValueChanged.AddListener(isOn =>
            {
                if (isOn)
                {
                    Debug.Log($"Setting item for slot '{_activeSlot.name}' to be '{item.Name}'");

                    Tooltips.HideTooltip();

                    _inventory.SetItemToSlotOnBoth(_activeSlot.name, item.Id);
                    _componentsContainer.SetActive(false);
                    _componentsContainer.transform.Clear();
                }
            });

            var tooltip = row.GetComponent<CraftingTooltip>();
            tooltip.OnPointerEnterForTooltip += pointerEventData =>
            {
                Tooltips.ShowTooltip(CraftingUi.GetItemDescription(item, false));
            };

            rowCounter++;
        }

        const int spacer = 5;
        var containerRectTrans = _componentsContainer.GetComponent<RectTransform>();
        containerRectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rowCounter * (rowRectTransform.rect.height + spacer));
    }

}
