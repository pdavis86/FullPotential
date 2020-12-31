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

            var components = new List<Attributes>();
            foreach (Transform transform in compContainer.transform)
            {
                if (transform.gameObject.activeInHierarchy && transform.gameObject.name.Contains("(Clone)"))
                {
                    transform.position = slotTemplate.position + new Vector3(components.Count * slotTemplateWidth, 0);
                    components.Add(transform.gameObject.GetComponent<ComponentAttributes>().Attributes);
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

            //todo: use this
            var handednessDropdown = header.Find("ResultHandedness").GetComponent<Dropdown>();

            Attributes resultingAttribues;
            if (selectedType == ItemBase.Spell)
            {
                resultingAttribues = resultFactory.Spell(components);
            }
            else
            {
                switch (selectedSubtype)
                {
                    case Weapon.Dagger: resultingAttribues = resultFactory.Dagger(components); break;
                    case Weapon.Spear: resultingAttribues = resultFactory.Spear(components); break;
                    case Weapon.Bow: resultingAttribues = resultFactory.Bow(components); break;
                    case Weapon.Crossbow: resultingAttribues = resultFactory.Crossbow(components); break;
                    case Weapon.Shield: resultingAttribues = resultFactory.Shield(components); break;

                    case Armor.Helm: resultingAttribues = resultFactory.Helm(components); break;
                    case Armor.Chest: resultingAttribues = resultFactory.Chest(components); break;
                    case Armor.Legs: resultingAttribues = resultFactory.Legs(components); break;
                    case Armor.Feet: resultingAttribues = resultFactory.Feet(components); break;
                    case Armor.Gloves: resultingAttribues = resultFactory.Gloves(components); break;
                    case Armor.Barrier: resultingAttribues = resultFactory.Barrier(components); break;

                    case Accessory.Amulet: resultingAttribues = resultFactory.Amulet(components); break;
                    case Accessory.Ring: resultingAttribues = resultFactory.Ring(components); break;
                    case Accessory.Belt: resultingAttribues = resultFactory.Belt(components); break;

                    default:
                        switch (selectedSubtype)
                        {
                            case Weapon.Axe: resultingAttribues = resultFactory.Axe(components); break;
                            case Weapon.Sword: resultingAttribues = resultFactory.Sword(components); break;
                            case Weapon.Hammer: resultingAttribues = resultFactory.Hammer(components); break;
                            case Weapon.Gun: resultingAttribues = resultFactory.Gun(components); break;
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

            textArea.text = $@"IsActivated: {resultingAttribues.IsActivated}
IsMultiShot {resultingAttribues.IsMultiShot}
Type {resultingAttribues.Type}
Strength {resultingAttribues.Strength}
Cost {resultingAttribues.Cost}
Range {resultingAttribues.Range}
Accuracy {resultingAttribues.Accuracy}
Speed {resultingAttribues.Speed}
Recovery {resultingAttribues.Recovery}
Duration {resultingAttribues.Duration}
";
        }
    }
}
