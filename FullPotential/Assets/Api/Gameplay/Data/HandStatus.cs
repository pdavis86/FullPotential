using System.Collections;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Items.SpellsAndGadgets;
using FullPotential.Api.Items.Weapons;
using FullPotential.Api.Registry.SpellsAndGadgets;

namespace FullPotential.Api.Gameplay.Data
{
    public class HandStatus
    {
        public string EquippedItemDescription { get; private set; }

        #region Weapons

        public WeaponItemBase EquippedWeapon { get; private set; }

        public bool IsReloading { get; set; }

        public IEnumerator RapidFireEnumerator { get; set; }

        #endregion

        #region SpellOrGadget

        public SpellOrGadgetItemBase EquippedSpellOrGadget { get; private set; }

        public IEnumerator ChargeEnumerator { get; set; }

        public ISpellOrGadgetBehaviour ActiveSpellOrGadgetBehaviour { get; set; }

        public IEnumerator CooldownEnumerator { get; set; }

        #endregion

        public void SetEquippedItem(ItemBase item, string description)
        {
            if (ActiveSpellOrGadgetBehaviour != null)
            {
                ActiveSpellOrGadgetBehaviour.Stop();
                ActiveSpellOrGadgetBehaviour = null;
            }

            EquippedItemDescription = description;

            switch (item)
            {
                case WeaponItemBase weapon:
                    EquippedWeapon = weapon;
                    EquippedSpellOrGadget = null;
                    break;

                case Spell spell:
                    EquippedWeapon = null;
                    EquippedSpellOrGadget = spell;
                    break;

                case Gadget gadget:
                    EquippedWeapon = null;
                    EquippedSpellOrGadget = gadget;
                    break;
            }
        }

        public bool IsConsumingMana()
        {
            return EquippedSpellOrGadget is Spell && ActiveSpellOrGadgetBehaviour != null;
        }

        public bool IsConsumingEnergy()
        {
            return EquippedSpellOrGadget is Gadget && ActiveSpellOrGadgetBehaviour != null;
        }
    }
}