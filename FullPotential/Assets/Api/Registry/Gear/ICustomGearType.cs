using System.Collections.Generic;

using FullPotential.Api.Gameplay.Inventory;
using FullPotential.Api.Items;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Localization;
using FullPotential.Api.Registry.Gameplay;

namespace FullPotential.Api.Registry.Gear
{
    public interface ICustomGearType : ICraftableType
    {
        SlotId SlotTypeId { get; }

        IEnumerable<ValuePoolTypeId> ValuePoolTypeIdsInUse { get; }

        string OverrideItemDescription(ItemBase item, ILocalizer localizer, LevelOfDetail levelOfDetail);
    }
}
