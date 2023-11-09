﻿using System;
using System.Collections.Generic;
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
                typeof(Accessories.SilverNecklace),
                typeof(Accessories.LeatherBelt),
                typeof(Accessories.SilverRing),

                typeof(Armor.LeatherHelmet),
                typeof(Armor.LeatherJerkin),
                typeof(Armor.LeatherGreaves),
                typeof(Armor.LeatherBoots),
                typeof(Armor.BasicWard),
                
                typeof(Targeting.ProjectileFlames),

                typeof(Shapes.WallOfFlames),
                typeof(Shapes.ZoneOfFlames),

                typeof(Loot.Scrap),
                typeof(Loot.Shard),
                typeof(Loot.Junk),

                typeof(Ammo.Arrow),
                typeof(Ammo.Bullet),

                typeof(Weapons.Axe),
                typeof(Weapons.Bow),
                typeof(Weapons.Crossbow),
                typeof(Weapons.Dagger),
                typeof(Weapons.Gun),
                typeof(Weapons.Hammer),
                typeof(Weapons.Shield),
                typeof(Weapons.Staff),
                typeof(Weapons.Sword),

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
            };
        }

        public IEnumerable<string> GetNetworkPrefabAddresses()
        {
            return Array.Empty<string>();
        }

        public void RegisterEventHandlers(IEventManager eventManager)
        {
            //eventManager.Subscribe(EventIds.ReloadStart, new Blah());
        }
    }

    //class Blah : IEventHandler
    //{
    //    public Func<IEventHandlerArgs, Task> BeforeEventAsync => Before;

    //    public Func<IEventHandlerArgs, Task> AfterEventAsync => After;

    //    private Task Before(IEventHandlerArgs args)
    //    {
    //        Debug.Log("Before cancelling");
    //        args.IsDefaultHandlerCancelled = true;
    //        return Task.CompletedTask;
    //    }

    //    private Task After(IEventHandlerArgs args)
    //    {
    //        Debug.Log("After");
    //        return Task.CompletedTask;
    //    }
    //}
}
