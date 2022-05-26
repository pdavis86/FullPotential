using System;
using System.Collections.Generic;
using FullPotential.Api.Registry;

namespace FullPotential.Standard
{
    public class Registration : IRegistrationSteps
    {
        public IEnumerable<Type> GetRegisterables()
        {
            return new[]
            {
                typeof(Armor.Helm),
                typeof(Armor.Chest),
                typeof(Armor.Legs),
                typeof(Armor.Feet),
                typeof(Armor.Barrier),

                typeof(Weapons.Axe),
                typeof(Weapons.Bow),
                typeof(Weapons.Crossbow),
                typeof(Weapons.Dagger),
                typeof(Weapons.Gun),
                typeof(Weapons.Hammer),
                typeof(Weapons.Shield),
                typeof(Weapons.Staff),
                typeof(Weapons.Sword),

                typeof(Accessories.Amulet),
                typeof(Accessories.Belt),
                typeof(Accessories.Ring),

                typeof(Loot.Scrap),
                typeof(Loot.Shard),

                typeof(SpellsAndGadgets.Shapes.Wall),
                typeof(SpellsAndGadgets.Shapes.Zone),
                
                typeof(SpellsAndGadgets.Targeting.Projectile),
                typeof(SpellsAndGadgets.Targeting.Beam),
                typeof(SpellsAndGadgets.Targeting.Self),
                typeof(SpellsAndGadgets.Targeting.Touch),

                typeof(Effects.Buffs.Courage),
                typeof(Effects.Buffs.Focus),
                typeof(Effects.Buffs.Haste),
                typeof(Effects.Buffs.LifeTap),
                typeof(Effects.Buffs.ManaTap),
                typeof(Effects.Buffs.Regeneration),
                typeof(Effects.Buffs.Strengthen),

                typeof(Effects.Debuffs.Distract),
                typeof(Effects.Debuffs.Fear),
                typeof(Effects.Debuffs.LifeDrain),
                typeof(Effects.Debuffs.ManaDrain),
                typeof(Effects.Debuffs.Poison),
                typeof(Effects.Debuffs.Slow),
                typeof(Effects.Debuffs.Weaken),

                typeof(Effects.Elements.Air),
                typeof(Effects.Elements.Earth),
                typeof(Effects.Elements.Fire),
                typeof(Effects.Elements.Ice),
                typeof(Effects.Elements.Lightning),
                typeof(Effects.Elements.Water),

                typeof(Effects.Movement.Attract),
                typeof(Effects.Movement.Blink),
                typeof(Effects.Movement.Launch),
                typeof(Effects.Movement.Repel),

                typeof(Effects.Support.Conjure),
                typeof(Effects.Support.Heal),
                typeof(Effects.Support.Hurt),
                typeof(Effects.Support.Reflect),
                typeof(Effects.Support.Float),
                typeof(Effects.Support.Summon),
            };
        }
    }
}
