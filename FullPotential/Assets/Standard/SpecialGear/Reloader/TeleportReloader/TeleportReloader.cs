using System;
using FullPotential.Api.Items;
using FullPotential.Api.Localization;
using FullPotential.Api.Registry.Gear;

namespace FullPotential.Standard.SpecialGear.Reloader.TeleportReloader
{
    public class TeleportReloader : ISpecialGearType
    {
        public const string TypeIdString = "80c23584-9c2b-45d0-9922-5a4cbc1616a1";

        private static readonly Guid Id = new Guid(TypeIdString);

        public Guid TypeId => Id;

        public string SlotIdString => SpecialSlots.RangedWeaponReloaderSlot.TypeIdString;

        public string OverrideItemDescription(Api.Items.Types.SpecialGear specialGear, ILocalizer localizer, LevelOfDetail levelOfDetail)
        {
            return null;
        }
    }
}
