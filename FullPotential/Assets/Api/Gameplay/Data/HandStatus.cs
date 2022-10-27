using System.Collections;
using FullPotential.Api.Registry.Base;
using FullPotential.Api.Registry.Gear;
using FullPotential.Api.Registry.SpellsAndGadgets;
using UnityEngine;

namespace FullPotential.Api.Gameplay.Data
{
    public class HandStatus
    {
        public string EquippedItemDescription { get; private set; }

        #region Weapons

        public bool IsReloading { get; set; }

        public Weapon EquippedWeapon { get; private set; }

        #endregion

        #region SpellOrGadget

        public SpellOrGadgetItemBase EquippedSpellOrGadget { get; private set; }

        public GameObject ActiveSpellOrGadgetGameObject { get; set; }

        public int ChargeCountdown { get; set; }

        public IEnumerator ChargeEnumerator { get; set; }

        public int CooldownCountdown { get; set; }

        public IEnumerator CooldownEnumerator { get; set; }

        #endregion

        public void SetEquippedItem(ItemBase item, string description)
        {
            EquippedItemDescription = description;

            switch (item)
            {
                case Weapon weapon:
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
            return EquippedSpellOrGadget is Spell && ActiveSpellOrGadgetGameObject != null;
        }

        public bool IsConsumingEnergy()
        {
            return EquippedSpellOrGadget is Gadget && ActiveSpellOrGadgetGameObject != null;
        }

        public bool StopConsumingResources()
        {
            if (ActiveSpellOrGadgetGameObject == null)
            {
                return false;
            }

            var behaviour = ActiveSpellOrGadgetGameObject.GetComponent<ISpellOrGadgetBehaviour>();

            if (behaviour == null)
            {
                return false;
            }

            behaviour.Stop();
            ActiveSpellOrGadgetGameObject = null;

            return true;
        }
    }
}