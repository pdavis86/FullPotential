using System;
using FullPotential.Api.Items;
using FullPotential.Api.Localization;
using FullPotential.Api.Registry.Gear;

namespace FullPotential.Standard.SpecialGear.Reloader.TeleportReloader
{
    public class TeleportReloader : ISpecialGear
    {
        public const string TypeIdString = "80c23584-9c2b-45d0-9922-5a4cbc1616a1";

        public Guid TypeId => new Guid(TypeIdString);

        public Guid SlotId => new Guid(SpecialSlots.RangedWeaponReloaderSlot.TypeIdString);

        public string OverrideItemDescription(Api.Items.Types.SpecialGear specialGear, ILocalizer localizer, LevelOfDetail levelOfDetail)
        {
            return null;
        }
    }
}
