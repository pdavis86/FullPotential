using System;
using System.Collections.Generic;

using FullPotential.Api.Items;
using FullPotential.Api.Localization;
using FullPotential.Api.Registry.Gear;
using FullPotential.Standard.Resources;

namespace FullPotential.Standard.SpecialGear.Reloader.ConsolidatorReloader
{
    public class ConsolidatorReloader : ISpecialGearType
    {
        public const string TypeIdString = "575ed70f-f5de-4ffa-93fb-a6c1cc404f30";

        private static readonly Guid Id = new Guid(TypeIdString);

        public Guid TypeId => Id;

        public string SlotIdString => SpecialSlots.RangedWeaponReloaderSlot.TypeIdString;

        public IEnumerable<string> ResourceTypeIdsInUse { get; } = new[] { Energy.TypeIdString };

        public string OverrideItemDescription(Api.Obsolete.Items.Types.SpecialGear specialGear, ILocalizer localizer, LevelOfDetail levelOfDetail)
        {
            return null;
        }
    }
}
