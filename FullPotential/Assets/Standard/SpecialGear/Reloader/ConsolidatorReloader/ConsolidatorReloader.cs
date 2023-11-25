using System;
using FullPotential.Api.Registry.Gear;

namespace FullPotential.Standard.SpecialGear.Reloader.ConsolidatorReloader
{
    public class ConsolidatorReloader : ISpecialGear
    {
        public const string TypeIdString = "575ed70f-f5de-4ffa-93fb-a6c1cc404f30";

        public Guid TypeId => new Guid(TypeIdString);

        public Guid SlotId => new Guid(SpecialSlots.RangedWeaponReloaderSlot.TypeIdString);
    }
}
