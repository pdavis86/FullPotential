using FullPotential.Api.Items;
using FullPotential.Api.Localization;
using FullPotential.Api.Obsolete.Items.Types;

namespace FullPotential.Api.Registry.Gear
{
    public interface ISpecialGearType : IRegisterableType
    {
        string SlotIdString { get; }

        string OverrideItemDescription(SpecialGear specialGear, ILocalizer localizer, LevelOfDetail levelOfDetail);
    }
}
