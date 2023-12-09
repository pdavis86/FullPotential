using System;
using FullPotential.Api.Items;
using FullPotential.Api.Localization;

namespace FullPotential.Api.Registry.Gear
{
    public interface ISpecialGear : IRegisterable
    {
        Guid SlotId { get; }

        string OverrideItemDescription(Items.Types.SpecialGear specialGear, ILocalizer localizer, LevelOfDetail levelOfDetail);
    }
}
