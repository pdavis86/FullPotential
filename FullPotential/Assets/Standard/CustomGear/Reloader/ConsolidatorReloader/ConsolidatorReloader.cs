using System;
using System.Collections.Generic;

using FullPotential.Api.Items;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Localization;
using FullPotential.Api.Registry.Gear;
using FullPotential.Standard.ValuePools;

using FullPotential.Api.CoreTypeIds;

namespace FullPotential.Standard.CustomGear.Reloader.ConsolidatorReloader
{
    public class ConsolidatorReloader : ICustomGearType
    {
        public const string TypeIdString = "575ed70f-f5de-4ffa-93fb-a6c1cc404f30";

        private static readonly Guid Id = new Guid(TypeIdString);

        public Guid TypeId => Id;

        public SlotId SlotId => FullPotential.Api.CoreTypeIds.SlotId.FromString(SpecialSlots.RangedWeaponReloaderSlot.TypeIdString);

        public IEnumerable<string> ValuePoolTypeIdsInUse { get; } = new[] { Energy.TypeIdString };

        public string OverrideItemDescription(ItemBase item, ILocalizer localizer, LevelOfDetail levelOfDetail)
        {
            return null;
        }
    }
}
