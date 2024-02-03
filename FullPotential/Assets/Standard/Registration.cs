using System;
using System.Collections.Generic;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Modding;
using UnityEngine;

// ReSharper disable UnusedType.Global

namespace FullPotential.Standard
{
    public class Registration : MonoBehaviour, IMod
    {
        public IEnumerable<Type> GetRegisterableTypes()
        {
            return new[]
            {
                typeof(Accessories.Ring),
                typeof(Accessories.Amulet),
                typeof(Accessories.Belt),
                typeof(Accessories.AutoAmmoBuyer.AutoAmmoBuyer),

                typeof(Ammo.Arrow),
                typeof(Ammo.Bullet),

                typeof(Effects.Buffs.Courage),
                typeof(Effects.Buffs.Endurance),
                typeof(Effects.Buffs.Focus),
                typeof(Effects.Buffs.Haste),
                typeof(Effects.Buffs.LifeTap),
                typeof(Effects.Buffs.ManaTap),
                typeof(Effects.Buffs.Regeneration),
                typeof(Effects.Buffs.Strengthen),
                typeof(Effects.Buffs.Surge),

                typeof(Effects.Debuffs.Distract),
                typeof(Effects.Debuffs.Fear),
                typeof(Effects.Debuffs.Lethargy),
                typeof(Effects.Debuffs.LifeDrain),
                typeof(Effects.Debuffs.ManaDrain),
                typeof(Effects.Debuffs.Poison),
                typeof(Effects.Debuffs.ShortCircuit),
                typeof(Effects.Debuffs.Slow),
                typeof(Effects.Debuffs.Weaken),

                typeof(Effects.Elements.Air),
                typeof(Effects.Elements.Earth),
                typeof(Effects.Elements.Fire),
                typeof(Effects.Elements.Ice),
                typeof(Effects.Elements.Lightning),
                typeof(Effects.Elements.Water),

                typeof(Effects.Movement.Attract),
                typeof(Effects.Movement.Hold),
                typeof(Effects.Movement.Launch),
                typeof(Effects.Movement.Lunge),
                typeof(Effects.Movement.Plummet),
                typeof(Effects.Movement.Repel),
                typeof(Effects.Movement.ShoveLeft),
                typeof(Effects.Movement.ShoveRight),
                typeof(Effects.Movement.StepBack),

                typeof(Effects.Support.Conjure),
                typeof(Effects.Support.Heal),
                typeof(Effects.Support.Hurt),
                typeof(Effects.Support.Reflect),
                typeof(Effects.Support.Float),
                typeof(Effects.Support.Summon),

                typeof(Loot.Scrap),
                typeof(Loot.Shard),
                typeof(Loot.Junk),

                typeof(Resources.BarrierChargeResource),

                typeof(SpecialGear.Barrier.Barrier),
                typeof(SpecialGear.Reloader.ConsolidatorReloader.ConsolidatorReloader),
                typeof(SpecialGear.Reloader.TeleportReloader.TeleportReloader),

                typeof(SpecialSlots.BarrierSlot),
                typeof(SpecialSlots.RangedWeaponReloaderSlot),

                typeof(Weapons.Axe),
                typeof(Weapons.Bow),
                typeof(Weapons.Crossbow),
                typeof(Weapons.Dagger),
                typeof(Weapons.Gun),
                typeof(Weapons.Hammer),
                typeof(Weapons.Shield),
                typeof(Weapons.Staff),
                typeof(Weapons.Sword),

                typeof(Armor.Helm),
                typeof(Armor.Chest),
                typeof(Armor.Legs),
                typeof(Armor.Feet),

                typeof(Targeting.PointToPoint),
                typeof(Targeting.Projectile),
                typeof(Targeting.Self),
                typeof(Targeting.Touch),

                typeof(Shapes.Wall),
                typeof(Shapes.Zone),
        };
        }

        public IEnumerable<Type> GetRegisterableVisuals()
        {
            return new[]
            {
                typeof(AccessoryVisuals.SilverNecklace),
                typeof(AccessoryVisuals.LeatherBelt),
                typeof(AccessoryVisuals.SilverRing),

                typeof(ArmorVisuals.LeatherHelmet),
                typeof(ArmorVisuals.LeatherJerkin),
                typeof(ArmorVisuals.LeatherGreaves),
                typeof(ArmorVisuals.LeatherBoots),

                typeof(ShapeVisuals.WallOfFlames),
                typeof(ShapeVisuals.ZoneOfFlames),

                typeof(TargetingVisuals.BeamOfFlames),
                typeof(TargetingVisuals.ProjectileFlames),

                typeof(WeaponVisuals.BasicAxe),
                typeof(WeaponVisuals.BasicBow),
                typeof(WeaponVisuals.BasicCrossbow),
                typeof(WeaponVisuals.BasicDagger),
                typeof(WeaponVisuals.BasicGun),
                typeof(WeaponVisuals.BasicHammer),
                typeof(WeaponVisuals.BasicShield),
                typeof(WeaponVisuals.BasicStaff),
                typeof(WeaponVisuals.BasicSword),

                typeof(SpecialGearVisuals.BasicWard),
            };
        }

        public IEnumerable<string> GetNetworkPrefabAddresses()
        {
            return new[]
            {
                Shapes.Wall.AddressablePath,
                Shapes.Zone.AddressablePath,
                Targeting.PointToPoint.AddressablePath,
                Targeting.Projectile.AddressablePath,
            };
        }

        public void RegisterEventHandlers(IEventManager eventManager)
        {
            eventManager.Subscribe<Accessories.AutoAmmoBuyer.ReloadEventHandler>(FighterBase.EventIdReload);
            eventManager.Subscribe<SpecialGear.Barrier.HealthChangeEventHandler>(LivingEntityBase.EventIdHealthChange);
            eventManager.Subscribe<SpecialGear.Reloader.ConsolidatorReloader.ReloadEventHandler>(FighterBase.EventIdReload);
            eventManager.Subscribe<SpecialGear.Reloader.TeleportReloader.ReloadEventHandler>(FighterBase.EventIdReload);
            eventManager.Subscribe<SpecialGear.Reloader.TeleportReloader.ShotFiredEventHandler>(FighterBase.EventIdShotFired);
            eventManager.Subscribe<SpecialGear.Reloader.SlotChangeEventHandler>(InventoryBase.EventIdSlotChange);
        }
    }
}
