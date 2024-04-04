using System;
using System.Text;
using FullPotential.Api.Items;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Localization;
using FullPotential.Api.Localization.Enums;
using FullPotential.Api.Obsolete;
using FullPotential.Api.Registry.Gear;

namespace FullPotential.Standard.SpecialGear.Barrier
{
    public class Barrier : ISpecialGear
    {
        public const string TypeIdString = "17a6e875-cccd-46f0-b525-fe15cfdd8096";

        public Guid TypeId => new Guid(TypeIdString);

        public Guid SlotId => new Guid(SpecialSlots.BarrierSlot.TypeIdString);

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
                localizer.TranslateFloat(GetRechargeDelay(specialGear)),
                UnitsType.Time);

            specialGear.AppendToDescription(
                sb,
                localizer,
                specialGear.Attributes.Speed,
                nameof(Attributes.Speed),
                nameof(Barrier),
                localizer.TranslateFloat(GetRechargeRate(specialGear)),
                UnitsType.UnitPerTime);

            return sb.ToString().Trim();
        }

        public static float GetRechargeDelay(Api.Items.Types.SpecialGear specialGear)
        {
            var returnValue = ItemForCombatBase.GetHighInLowOutInRange(specialGear.Attributes.Recovery, 0.5f, 5f);
            return returnValue;
        }

        public static int GetRechargeRate(Api.Items.Types.SpecialGear specialGear)
        {
            var returnValue = (int)ItemForCombatBase.GetHighInLowOutInRange(specialGear.Attributes.Speed, 1, 10);
            return returnValue;
        }
    }
}
