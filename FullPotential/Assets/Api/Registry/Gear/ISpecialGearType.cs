using System.Collections.Generic;

using FullPotential.Api.Items;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Localization;

namespace FullPotential.Api.Registry.Gear
{
    public interface ISpecialGearType : ICraftableType
    {
        string SlotIdString { get; }

        IEnumerable<string> ResourceTypeIdsInUse { get; }

        string OverrideItemDescription(ItemBase item, ILocalizer localizer, LevelOfDetail levelOfDetail);
    }
}
