using System;
using System.Collections.Generic;
using System.Text;

using FullPotential.Api.Gameplay.Attributes;
using FullPotential.Api.Gameplay.Inventory;
using FullPotential.Api.Items;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Localization;
using FullPotential.Api.Registry.Gameplay;
using FullPotential.Api.Registry.Gear;
using FullPotential.Api.Utilities;
using FullPotential.Api.Utilities.Extensions;
using FullPotential.Standard.ValuePools;

namespace FullPotential.Standard.CustomGear.Barrier
{
    public class Barrier : ICustomGearType
    {
        public const string TypeIdString = "17a6e875-cccd-46f0-b525-fe15cfdd8096";

        public Guid TypeId { get; } = new Guid(TypeIdString);

        public SlotId SlotTypeId => SlotId.FromString(SpecialSlots.BarrierSlot.TypeIdString);

        public IEnumerable<ValuePoolTypeId> ValuePoolTypeIdsInUse { get; } = new[] { Energy.Id, BarrierChargeValuePool.Id };

        public string OverrideItemDescription(ItemBase item, ILocalizer localizer, LevelOfDetail levelOfDetail)
        {
            var customGear = item as Items.CustomGear;

            var sb = new StringBuilder();

            if (levelOfDetail == LevelOfDetail.Full)
            {
                sb.Append($"{localizer.Translate(TranslationType.Item, nameof(customGear.Name))}: {customGear.Name}" + "\n");

                sb.Append($"{localizer.Translate(TranslationType.Item, nameof(customGear.RegistryType))}: {localizer.Translate(this)}" + "\n");
            }

            var valuePoolConsumptionType = localizer.Translate(customGear.ValuePoolType);
            sb.Append($"{localizer.Translate(TranslationType.Item, nameof(customGear.ValuePoolType))}: {valuePoolConsumptionType}" + "\n");

            customGear.AppendToDescription(
                sb,
                localizer,
                customGear.Attributes.GetValueOrZero(Recovery.Id),
                nameof(Recovery),
                nameof(Barrier),
                localizer.Translate(GetRechargeDelay(customGear)),
                UnitsType.Time);

            customGear.AppendToDescription(
                sb,
                localizer,
                customGear.Attributes.GetValueOrZero(Speed.Id),
                nameof(Speed),
                nameof(Barrier),
                localizer.Translate(GetRechargeRate(customGear)),
                UnitsType.UnitPerTime);

            return sb.ToString().Trim();
        }

        public static float GetRechargeDelay(Items.CustomGear customGear)
        {
            var returnValue = MathsHelper.GetHighInLowOutInRange(customGear.Attributes.GetValueOrZero(Recovery.Id), 0.5f, 5f);
            return returnValue;
        }

        public static int GetRechargeRate(Items.CustomGear customGear)
        {
            var returnValue = (int)MathsHelper.GetHighInLowOutInRange(customGear.Attributes.GetValueOrZero(Speed.Id), 1, 10);
            return returnValue;
        }
    }
}
