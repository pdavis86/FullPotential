using System;
using System.Collections.Generic;

using FullPotential.Api.Gameplay.Inventory;
using FullPotential.Api.Items;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Localization;
using FullPotential.Api.Registry.Gameplay;
using FullPotential.Api.Registry.Gear;
using FullPotential.Standard.ValuePools;

namespace FullPotential.Standard.CustomGear.Reloader.TeleportReloader
{
    public class TeleportReloader : ICustomGearType
    {
        public const string TypeIdString = "80c23584-9c2b-45d0-9922-5a4cbc1616a1";

        public Guid TypeId { get; } = new Guid(TypeIdString);

        public SlotId SlotTypeId => SlotId.FromString(SpecialSlots.RangedWeaponReloaderSlot.TypeIdString);

        public IEnumerable<ValuePoolTypeId> ValuePoolTypeIdsInUse { get; } = new[] { Energy.Id };

        public string OverrideItemDescription(ItemBase item, ILocalizer localizer, LevelOfDetail levelOfDetail)
        {
            return null;
        }
    }
}
