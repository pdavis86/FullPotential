using Assets.Scripts.Crafting.Results;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Crafting
{
    public class UiHelper
    {
        private static Transform _componentsContainer;
        private static RectTransform _componentTemplateRect;
        private static Text _textArea;
        private static Dropdown _typeDropdown;
        private static Dropdown _subTypeDropdown;
        private static Dropdown _handednessDropdown;

        private UiHelper() { }

        private static UiHelper _instance;
        public static UiHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    var craftingTransform = GameManager.Instance.GameObjects.UiCrafting.transform;

                    _componentsContainer = craftingTransform.Find("ComponentsScrollView").Find("ComponentsContainer");

                    var componentTemplateTransform = _componentsContainer.Find("ComponentTemplate");
                    _componentTemplateRect = componentTemplateTransform.GetComponent<RectTransform>();

                    _textArea = craftingTransform.Find("ResultingItem").Find("Text").GetComponent<Text>();

                    var header = craftingTransform.Find("Header");
                    _typeDropdown = header.Find("ResultType").GetComponent<Dropdown>();
                    _subTypeDropdown = header.Find("ResultSubType").GetComponent<Dropdown>();
                    _handednessDropdown = header.Find("ResultHandedness").GetComponent<Dropdown>();

                    _instance = new UiHelper();
                }

                return _instance;
            }
        }

        public void UpdateResults()
        {
            var components = new List<ItemBase>();
            foreach (Transform transform in _componentsContainer.transform)
            {
                if (transform.gameObject.activeInHierarchy && transform.gameObject.name.Contains("(Clone)"))
                {
                    transform.position = _componentTemplateRect.position + new Vector3(components.Count * _componentTemplateRect.rect.width, 0);
                    components.Add(transform.gameObject.GetComponent<ComponentProperties>().Properties);
                }
            }

            if (components.Count == 0)
            {
                _textArea.text = null;
                return;
            }

            var selectedType = _typeDropdown.options[_typeDropdown.value].text;
            var selectedSubtype = _subTypeDropdown.options.Count > 0 ? _subTypeDropdown.options[_subTypeDropdown.value].text : null;
            var isTwoHanded = _handednessDropdown.options.Count > 0 && _handednessDropdown.options[_handednessDropdown.value].text == Weapon.TwoHanded;



            var craftedThing = CmdGetCraftedItem(components, selectedType, selectedSubtype, isTwoHanded);

            ////copy a row
            //var container = transform.Find("container");
            //var template = transform.Find("rowContaingTheTextItems");
            //template.gameObject.SetActive(false);

            //var rowHeight = 20f;
            //for (var i = 0; i < 100; i++)
            //{
            //    var newRow = Instantiate(template, container);
            //    var newRowTransform = newRow.GetComponent<RectTransform>();
            //    newRowTransform.anchoredPosition = new Vector2(0, -rowHeight * i);
            //    newRow.gameObject.SetActive(true);
            //}

            //todo: work on the UI. I think it needs to be a table showing which aspects come from which loot
            //todo: requirements e.g. strength, speed, accuracy, 6 scrap or less

            var sb = new StringBuilder();

            sb.Append($"Name: {craftedThing.Name}\n");
            if (craftedThing.Attributes.IsActivated) { sb.Append("IsActivated: true\n"); }
            if (craftedThing.Attributes.IsAutomatic) { sb.Append("IsAutomatic: true\n"); }
            if (craftedThing.Attributes.IsSoulbound) { sb.Append("IsSoulbound: true\n"); }
            if (craftedThing.Attributes.ExtraAmmoPerShot > 0) { sb.Append($"ExtraAmmoPerShot: {craftedThing.Attributes.ExtraAmmoPerShot}\n"); }
            if (craftedThing.Attributes.Strength > 0) { sb.Append($"Strength: {craftedThing.Attributes.Strength}\n"); }
            if (craftedThing.Attributes.Cost > 0) { sb.Append($"Cost: {craftedThing.Attributes.Cost}\n"); }
            if (craftedThing.Attributes.Range > 0) { sb.Append($"Range: {craftedThing.Attributes.Range}\n"); }
            if (craftedThing.Attributes.Accuracy > 0) { sb.Append($"Accuracy: {craftedThing.Attributes.Accuracy}\n"); }
            if (craftedThing.Attributes.Speed > 0) { sb.Append($"Speed: {craftedThing.Attributes.Speed}\n"); }
            if (craftedThing.Attributes.Recovery > 0) { sb.Append($"Recovery: {craftedThing.Attributes.Recovery}\n"); }
            if (craftedThing.Attributes.Duration > 0) { sb.Append($"Duration: {craftedThing.Attributes.Duration}\n"); }
            if (craftedThing.Effects.Count > 0) { sb.Append($"Effects: {string.Join(", ", craftedThing.Effects)}"); }

            _textArea.text = sb.ToString();
        }

        //todo: this needs moving to a NetworkBehaviour to use - [command]
        private ItemBase CmdGetCraftedItem(List<ItemBase> components, string selectedType, string selectedSubtype, bool isTwoHanded)
        {
            //todo: check the components are actually in the player's invesntory

            var resultFactory = GameManager.Instance.ResultFactory;

            ItemBase craftedThing;
            if (selectedType == ChooseCraftingType.CraftingTypeSpell)
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
}
