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

                typeof(Standard.Weapons.Axe),
                typeof(Standard.Weapons.Bow),
                typeof(Standard.Weapons.Crossbow),
                typeof(Standard.Weapons.Dagger),
                typeof(Standard.Weapons.Gun),
                typeof(Standard.Weapons.Hammer),
                typeof(Standard.Weapons.Shield),
                typeof(Standard.Weapons.Staff),
                typeof(Standard.Weapons.Sword),

                typeof(Standard.Loot.Scrap),
                typeof(Standard.Loot.Shard),

                typeof(Standard.Effects.Buffs.LifeTap),

                typeof(Standard.Effects.Debuffs.LifeDrain)
            };
        }
    }
}
