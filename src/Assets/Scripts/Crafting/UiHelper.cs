using Assets.Scripts.Attributes;
using Assets.Scripts.Crafting.Results;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Crafting
{
    public static class UiHelper
    {
        public static void UpdateResults(Transform CraftingTransform, ResultFactory resultFactory)
        {
            var compContainer = CraftingTransform.Find("ComponentsScrollView").Find("ComponentsContainer");
            var slotTemplate = compContainer.Find("ComponentTemplate");
            var slotTemplateWidth = slotTemplate.GetComponent<RectTransform>().rect.width;

            var components = new List<CraftableBase>();
            foreach (Transform transform in compContainer.transform)
            {
                if (transform.gameObject.activeInHierarchy && transform.gameObject.name.Contains("(Clone)"))
                {
                    transform.position = slotTemplate.position + new Vector3(components.Count * slotTemplateWidth, 0);
                    components.Add(transform.gameObject.GetComponent<ComponentProperties>().Properties);
                }
            }

            var textArea = CraftingTransform.Find("ResultingItem").Find("Text").GetComponent<Text>();

            if (components.Count == 0)
            {
                textArea.text = null;
                return;
            }

            var header = CraftingTransform.Find("Header");

            var typeDropdown = header.Find("ResultType").GetComponent<Dropdown>();
            var selectedType = typeDropdown.options[typeDropdown.value].text;

            var subTypeDropdown = header.Find("ResultSubType").GetComponent<Dropdown>();
            var selectedSubtype = subTypeDropdown.options.Count > 0 ? subTypeDropdown.options[subTypeDropdown.value].text : null;

            var handednessDropdown = header.Find("ResultHandedness").GetComponent<Dropdown>();
            var isTwoHanded = handednessDropdown.options.Count > 0 && handednessDropdown.options[handednessDropdown.value].text == Weapon.TwoHanded;

            var craftedThing = GetCraftedItem(resultFactory, components, selectedType, selectedSubtype, isTwoHanded);

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

            //todo: only show values if >0 or true

            //todo: work on the UI. I think it needs to be a table showing which aspects come from which loot

            var sb = new StringBuilder();

            if (craftedThing.Attributes.IsActivated) { sb.Append("IsActivated: true\n"); }
            if (craftedThing.Attributes.IsAutomatic) { sb.Append("IsAutomatic: true\n"); }
            if (craftedThing.Attributes.IsSoulbound) { sb.Append("IsSoulbound: true\n"); }
            if (craftedThing.Attributes.ExtraAmmoPerShot > 0) { sb.Append("ExtraAmmoPerShot: " + craftedThing.Attributes.ExtraAmmoPerShot + "\n"); }
            if (craftedThing.Attributes.Strength > 0) { sb.Append("Strength: " + craftedThing.Attributes.Strength + "\n"); }
            if (craftedThing.Attributes.Cost > 0) { sb.Append("Cost: " + craftedThing.Attributes.Cost + "\n"); }
            if (craftedThing.Attributes.Range > 0) { sb.Append("Range: " + craftedThing.Attributes.Range + "\n"); }
            if (craftedThing.Attributes.Accuracy > 0) { sb.Append("Accuracy: " + craftedThing.Attributes.Accuracy + "\n"); }
            if (craftedThing.Attributes.Speed > 0) { sb.Append("Speed: " + craftedThing.Attributes.Speed + "\n"); }
            if (craftedThing.Attributes.Recovery > 0) { sb.Append("Recovery: " + craftedThing.Attributes.Recovery + "\n"); }
            if (craftedThing.Attributes.Duration > 0) { sb.Append("Duration: " + craftedThing.Attributes.Duration + "\n"); }
            if (craftedThing.Effects.Count > 0) { sb.Append("Effects: " + string.Join(", ", craftedThing.Effects)); }

            textArea.text = sb.ToString();
        }

        [ServerSideOnlyTemp]
        private static CraftableBase GetCraftedItem(ResultFactory resultFactory, List<CraftableBase> components, string  selectedType, string selectedSubtype, bool isTwoHanded)
        {
            //todo: check the components are actually in the player's invesntory

            CraftableBase craftedThing;
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
