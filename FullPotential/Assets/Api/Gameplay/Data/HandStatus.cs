using FullPotential.Api.Registry.Base;
using FullPotential.Api.Registry.Gear;
using FullPotential.Api.Registry.SpellsAndGadgets;
using UnityEngine;

namespace FullPotential.Api.Gameplay.Data
{
    public class HandStatus
    {
        public bool IsReloading { get; set; }
        
        public string EquippedItemDescription { get; private set; }

        public ItemBase EquippedItem { get; private set; }
        public Weapon EquippedWeapon { get; private set; }
        public Spell EquippedSpell { get; private set; }
        public Gadget EquippedGadget { get; private set; }


        //todo: Do continuous resource drain better
        public SpellOrGadgetItemBase ContinuousSpellOrGadgetItem { get; set; }
        public GameObject SpellOrGadgetGameObject { get; set; }
        public ISpellOrGadgetBehaviour SpellOrGadgetBehaviour { get; set; }


        public void SetEquippedItem(ItemBase item, string description)
        {
            EquippedItem = item;
            EquippedItemDescription = description;

            switch (item)
            {
                case Weapon weapon:
                    EquippedWeapon = weapon;
                    EquippedSpell = null;
                    EquippedGadget = null;
                    ContinuousSpellOrGadgetItem = null;
                    break;

                case Spell spell:
                    EquippedWeapon = null;
                    EquippedSpell = spell;
                    EquippedGadget = null;
                    ContinuousSpellOrGadgetItem = null;
                    break;

                case Gadget gadget:
                    EquippedWeapon = null;
                    EquippedSpell = null;
                    EquippedGadget = gadget;
                    ContinuousSpellOrGadgetItem = null;
                    break;
            }
        }
    }
}