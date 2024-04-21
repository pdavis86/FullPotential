using FullPotential.Api.Items;
using FullPotential.Api.Localization;

namespace FullPotential.Api.Registry.Gear
{
    public interface ISpecialGearType : IRegisterableType
    {
        string SlotIdString { get; }

        string OverrideItemDescription(Items.Types.SpecialGear specialGear, ILocalizer localizer, LevelOfDetail levelOfDetail);
    }
}
