using System;
using FullPotential.Api.Items;
using FullPotential.Api.Localization;
using FullPotential.Api.Registry.Gear;

namespace FullPotential.Standard.SpecialGear.Reloader.ConsolidatorReloader
{
    public class ConsolidatorReloader : ISpecialGearType
    {
        public const string TypeIdString = "575ed70f-f5de-4ffa-93fb-a6c1cc404f30";

        private static readonly Guid Id = new Guid(TypeIdString);

        public Guid TypeId => Id;

        public string SlotIdString => SpecialSlots.RangedWeaponReloaderSlot.TypeIdString;

        public string OverrideItemDescription(Api.Items.Types.SpecialGear specialGear, ILocalizer localizer, LevelOfDetail levelOfDetail)
        {
            return null;
        }
    }
}
