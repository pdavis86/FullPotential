using System.Collections.Generic;

using FullPotential.Api.Items;
using FullPotential.Api.Localization;
using FullPotential.Api.Obsolete.Items.Types;

namespace FullPotential.Api.Registry.Gear
{
    public interface ISpecialGearType : IRegisterableType
    {
        string SlotIdString { get; }

        // todo: OverrideItemDescription feels like it should be in a parent interface
        string OverrideItemDescription(SpecialGear specialGear, ILocalizer localizer, LevelOfDetail levelOfDetail);

        IEnumerable<string> ResourceTypeIdsInUse { get; }
    }
}
