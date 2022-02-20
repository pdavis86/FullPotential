using FullPotential.Api.Registry.SpellsAndGadgets;
using UnityEngine;

namespace FullPotential.Api.Gameplay.Data
{
    public class PlayerHandStatus
    {
        public bool IsReloading;
        public int Ammo;
        public int AmmoMax;
        public SpellOrGadgetItemBase SpellOrGadgetItem;
        public GameObject SpellOrGadgetGameObject;
        public ISpellOrGadgetBehaviour SpellOrGadgetBehaviour;
    }
}