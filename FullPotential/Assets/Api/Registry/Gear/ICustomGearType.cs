using System.Collections.Generic;

using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Items;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Localization;

namespace FullPotential.Api.Registry.Gear
{
    public interface ICustomGearType : ICraftableType
    {
        SlotId SlotId { get; }

        IEnumerable<string> ValuePoolTypeIdsInUse { get; }

        string OverrideItemDescription(ItemBase item, ILocalizer localizer, LevelOfDetail levelOfDetail);
    }
}
