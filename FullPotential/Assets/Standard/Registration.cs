using FullPotential.Api;
using System;
using System.Collections.Generic;

namespace FullPotential.Standard
{
    public class Registration : IRegistrationSteps
    {
        public IEnumerable<Type> GetRegisterables()
        {
            return new[]
            {
                typeof(Accessories.Amulet),
                typeof(Accessories.Belt),
                typeof(Accessories.Ring),

                typeof(Armor.Helm),
                typeof(Armor.Chest),
                typeof(Armor.Legs),
                typeof(Armor.Feet),
                typeof(Armor.Barrier),

                typeof(Effects.Buffs.Courage),
                typeof(Effects.Buffs.Focus),
                typeof(Effects.Buffs.Haste),
                typeof(Effects.Buffs.LifeTap),
                typeof(Effects.Buffs.ManaTap),
                typeof(Effects.Buffs.Regeneration),
                typeof(Effects.Buffs.Strength),

                typeof(Effects.Debuffs.Distract),
                typeof(Effects.Debuffs.Fear),
                typeof(Effects.Debuffs.LifeDrain),
                typeof(Effects.Debuffs.ManaDrain),
                typeof(Effects.Debuffs.Poison),
                typeof(Effects.Debuffs.Slow),
                typeof(Effects.Debuffs.Weakness),

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

                typeof(Effects.Support.Absorb),
                typeof(Effects.Support.Deflect),
                typeof(Effects.Support.Heal),
                typeof(Effects.Support.Reflect),
                typeof(Effects.Support.Soften),
                typeof(Effects.Support.Summon),

                typeof(Loot.Scrap),
                typeof(Loot.Shard),

                typeof(Weapons.Axe),
                typeof(Weapons.Bow),
                typeof(Weapons.Crossbow),
                typeof(Weapons.Dagger),
                typeof(Weapons.Gun),
                typeof(Weapons.Hammer),
                typeof(Weapons.Shield),
                typeof(Weapons.Staff),
                typeof(Weapons.Sword)
            };
        }
    }
}
