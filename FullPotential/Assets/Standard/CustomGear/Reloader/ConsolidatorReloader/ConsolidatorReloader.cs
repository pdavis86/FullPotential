using System;
using System.Collections.Generic;

using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Items;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Localization;
using FullPotential.Api.Registry.Gear;
using FullPotential.Api.Registry.Gameplay;
using FullPotential.Standard.ValuePools;

namespace FullPotential.Standard.CustomGear.Reloader.ConsolidatorReloader
{
    public class ConsolidatorReloader : ICustomGearType
    {
        public const string TypeIdString = "575ed70f-f5de-4ffa-93fb-a6c1cc404f30";

        public Guid TypeId { get; } = new Guid(TypeIdString);

        public SlotId SlotId => SlotId.FromString(SpecialSlots.RangedWeaponReloaderSlot.TypeIdString);

        public IEnumerable<ValuePoolTypeId> ValuePoolTypeIdsInUse { get; } = new[] { Energy.Id };

        public string OverrideItemDescription(ItemBase item, ILocalizer localizer, LevelOfDetail levelOfDetail)
        {
            return null;
        }
    }
}
