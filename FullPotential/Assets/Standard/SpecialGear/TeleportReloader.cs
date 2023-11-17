using System;
using System.Collections.Generic;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Registry.Gear;

namespace FullPotential.Standard.SpecialGear
{
    public class TeleportReloader : ISpecialGear
    {
        public Guid TypeId => new Guid("80c23584-9c2b-45d0-9922-5a4cbc1616a1");

        public string TypeName => nameof(ConsolidatorReloader);

        public Guid SlotId => new Guid(SpecialSlots.RangedWeaponReloader.TypeIdString);

        public Dictionary<string, IEventHandler> EventHandlers { get; } = new Dictionary<string, IEventHandler>
        {
            {EventIds.FighterShotFired, new TeleportReloaderEventHandler()}
        };
    }
}
