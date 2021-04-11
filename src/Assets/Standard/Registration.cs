using Assets.ApiScripts;
using System;
using System.Collections.Generic;

namespace Assets.Standard
{
    public class Registration : IRegistrationSteps
    {
        public IEnumerable<Type> GetRegisterables()
        {
            return new[]
            {
                typeof(Standard.Accessories.Amulet),
                typeof(Standard.Accessories.Belt),
                typeof(Standard.Accessories.Ring),

                typeof(Standard.Armor.Helm),
                typeof(Standard.Armor.Chest),
                typeof(Standard.Armor.Legs),
                typeof(Standard.Armor.Feet),
                typeof(Standard.Armor.Barrier),

                typeof(Standard.Effects.Buffs.Courage),
                typeof(Standard.Effects.Buffs.Focus),
                typeof(Standard.Effects.Buffs.Haste),
                typeof(Standard.Effects.Buffs.LifeTap),
                typeof(Standard.Effects.Buffs.ManaTap),
                typeof(Standard.Effects.Buffs.Regeneration),
                typeof(Standard.Effects.Buffs.Strengthen),

                typeof(Standard.Effects.Debuffs.Distract),
                typeof(Standard.Effects.Debuffs.Fear),
                typeof(Standard.Effects.Debuffs.LifeDrain),
                typeof(Standard.Effects.Debuffs.ManaDrain),
                typeof(Standard.Effects.Debuffs.Poison),
                typeof(Standard.Effects.Debuffs.Slow),
                typeof(Standard.Effects.Debuffs.Weaken),

                typeof(Standard.Effects.Elements.Air),
                typeof(Standard.Effects.Elements.Earth),
                typeof(Standard.Effects.Elements.Fire),
                typeof(Standard.Effects.Elements.Ice),
                typeof(Standard.Effects.Elements.Lightning),
                typeof(Standard.Effects.Elements.Water),

                typeof(Standard.Effects.Support.Absorb),
                typeof(Standard.Effects.Support.Blink),
                typeof(Standard.Effects.Support.Deflect),
                typeof(Standard.Effects.Support.Heal),
                typeof(Standard.Effects.Support.Leap),
                typeof(Standard.Effects.Support.Soften),

                typeof(Standard.Loot.Scrap),
                typeof(Standard.Loot.Shard),

                typeof(Standard.Weapons.Axe),
                typeof(Standard.Weapons.Bow),
                typeof(Standard.Weapons.Crossbow),
                typeof(Standard.Weapons.Dagger),
                typeof(Standard.Weapons.Gun),
                typeof(Standard.Weapons.Hammer),
                typeof(Standard.Weapons.Shield),
                typeof(Standard.Weapons.Staff),
                typeof(Standard.Weapons.Sword)
            };
        }
    }
}
