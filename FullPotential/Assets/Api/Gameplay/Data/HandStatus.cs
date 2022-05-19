using FullPotential.Api.Registry.Base;
using FullPotential.Api.Registry.SpellsAndGadgets;
using UnityEngine;

namespace FullPotential.Api.Gameplay.Data
{
    public class HandStatus
    {
        public bool IsLeftHand;
        public bool IsReloading;
        public int Ammo;
        public int AmmoMax;
        public ItemBase EquippedItem;
        public SpellOrGadgetItemBase SpellOrGadgetItem;
        public GameObject SpellOrGadgetGameObject;
        public ISpellOrGadgetBehaviour SpellOrGadgetBehaviour;
    }
}