using Assets.Scripts.Ui.Crafting.Items;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Ui.Crafting
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

            CraftableBase craftedThing;
            if (selectedType == ChooseCraftingType.Spell)
            {
                craftedThing = resultFactory.Spell(components);
            }
            else
            {
                switch (selectedSubtype)
                {
                    case Weapon.Dagger: craftedThing = resultFactory.MeleeWeapon(Weapon.Dagger, components, false); break;
                    case Weapon.Spear: craftedThing = resultFactory.MeleeWeapon(Weapon.Spear, components, true); break;
                    case Weapon.Bow: craftedThing = resultFactory.RangedWeapon(Weapon.Bow, components, true); break;
                    case Weapon.Crossbow: craftedThing = resultFactory.RangedWeapon(Weapon.Crossbow, components, true); break;
                    case Weapon.Shield: craftedThing = resultFactory.Shield(components); break;

                    case Armor.Helm: craftedThing = resultFactory.Armor(Armor.Helm, components); break;
                    case Armor.Chest: craftedThing = resultFactory.Armor(Armor.Chest, components); break;
                    case Armor.Legs: craftedThing = resultFactory.Armor(Armor.Legs, components); break;
                    case Armor.Feet: craftedThing = resultFactory.Armor(Armor.Feet, components); break;
                    case Armor.Gloves: craftedThing = resultFactory.Armor(Armor.Gloves, components); break;
                    case Armor.Barrier: craftedThing = resultFactory.Barrier(components); break;

                    case Accessory.Amulet: craftedThing = resultFactory.Accessory(Accessory.Amulet, components); break;
                    case Accessory.Ring: craftedThing = resultFactory.Accessory(Accessory.Ring, components); break;
                    case Accessory.Belt: craftedThing = resultFactory.Accessory(Accessory.Belt, components); break;

                    default:
                        var handednessDropdown = header.Find("ResultHandedness").GetComponent<Dropdown>();
                        var isTwoHanded = handednessDropdown.options.Count > 0 && handednessDropdown.options[handednessDropdown.value].text == Weapon.TwoHanded;

                        switch (selectedSubtype)
                        {
                            case Weapon.Axe: craftedThing = resultFactory.MeleeWeapon(Weapon.Axe, components, isTwoHanded); break;
                            case Weapon.Sword: craftedThing = resultFactory.MeleeWeapon(Weapon.Sword, components, isTwoHanded); break;
                            case Weapon.Hammer: craftedThing = resultFactory.MeleeWeapon(Weapon.Hammer, components, isTwoHanded); break;
                            case Weapon.Gun: craftedThing = resultFactory.RangedWeapon(Weapon.Gun, components, isTwoHanded); break;
                            default:
                                throw new System.Exception("Invalid weapon type");
                        }
                        break;
                }
            }

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

            textArea.text = $@"IsActivated: {craftedThing.Attributes.IsActivated}
IsAutomatic {craftedThing.Attributes.IsAutomatic}
IsSoulbound {craftedThing.Attributes.IsSoulbound}
ExtraAmmoPerShot {craftedThing.Attributes.ExtraAmmoPerShot}
Strength {craftedThing.Attributes.Strength}
Cost {craftedThing.Attributes.Cost}
Range {craftedThing.Attributes.Range}
Accuracy {craftedThing.Attributes.Accuracy}
Speed {craftedThing.Attributes.Speed}
Recovery {craftedThing.Attributes.Recovery}
Duration {craftedThing.Attributes.Duration}
Effects {string.Join(", ", craftedThing.Effects ?? new List<string>())}
";
        }

    }
}
