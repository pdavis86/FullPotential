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

                typeof(Effects.Support.Absorb),
                typeof(Effects.Support.Blink),
                typeof(Effects.Support.Deflect),
                typeof(Effects.Support.Heal),
                typeof(Effects.Support.Leap),
                typeof(Effects.Support.Soften),

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
