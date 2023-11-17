using System;
using System.Collections.Generic;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Registry.Gear;

namespace FullPotential.Standard.SpecialGear
{
    public class ConsolidatorReloader : ISpecialGear
    {
        public Guid TypeId => new Guid("575ed70f-f5de-4ffa-93fb-a6c1cc404f30");

        public string TypeName => nameof(ConsolidatorReloader);

        public Guid SlotId => SpecialGearSlots.Reloader;

        public Dictionary<string, IEventHandler> EventHandlers { get; } = new Dictionary<string, IEventHandler>
        {
            {EventIds.FighterReloadBegin, new ConsolidatorReloaderEventHandler()}
        };
    }
}
