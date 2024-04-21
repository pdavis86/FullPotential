using System;
using System.Text;
using FullPotential.Api.Items;
using FullPotential.Api.Localization;
using FullPotential.Api.Localization.Enums;
using FullPotential.Api.Obsolete;
using FullPotential.Api.Registry.Gear;
using FullPotential.Api.Utilities;

namespace FullPotential.Standard.SpecialGear.Barrier
{
    public class Barrier : ISpecialGearType
    {
        public const string TypeIdString = "17a6e875-cccd-46f0-b525-fe15cfdd8096";

        private static readonly Guid Id = new Guid(TypeIdString);

        public Guid TypeId => Id;

        public string SlotIdString => SpecialSlots.BarrierSlot.TypeIdString;

        public string OverrideItemDescription(Api.Items.Types.SpecialGear specialGear, ILocalizer localizer, LevelOfDetail levelOfDetail)
        {
            var sb = new StringBuilder();

            if (levelOfDetail == LevelOfDetail.Full)
            {
                sb.Append($"{localizer.Translate(TranslationType.Item, nameof(specialGear.Name))}: {specialGear.Name}" + "\n");

                sb.Append($"{localizer.Translate(TranslationType.Item, nameof(specialGear.RegistryType))}: {localizer.Translate(TranslationType.ItemType, GetType().Name)}" + "\n");
            }

            var resourceConsumptionType = localizer.Translate(specialGear.ResourceType);
            sb.Append($"{localizer.Translate(TranslationType.Item, nameof(specialGear.ResourceType))}: {resourceConsumptionType}" + "\n");

            specialGear.AppendToDescription(
                sb,
                localizer,
                specialGear.Attributes.Recovery,
                nameof(Attributes.Recovery),
                nameof(Barrier),
                localizer.Translate(GetRechargeDelay(specialGear)),
                UnitsType.Time);

            specialGear.AppendToDescription(
                sb,
                localizer,
                specialGear.Attributes.Speed,
                nameof(Attributes.Speed),
                nameof(Barrier),
                localizer.Translate(GetRechargeRate(specialGear)),
                UnitsType.UnitPerTime);

            return sb.ToString().Trim();
        }

        public static float GetRechargeDelay(Api.Items.Types.SpecialGear specialGear)
        {
            var returnValue = MathsHelper.GetHighInLowOutInRange(specialGear.Attributes.Recovery, 0.5f, 5f);
            return returnValue;
        }

        public static int GetRechargeRate(Api.Items.Types.SpecialGear specialGear)
        {
            var returnValue = (int)MathsHelper.GetHighInLowOutInRange(specialGear.Attributes.Speed, 1, 10);
            return returnValue;
        }
    }
}
