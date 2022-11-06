using System.Linq;
using System.Text;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Gameplay.Items;
using FullPotential.Api.Localization;
using FullPotential.Api.Localization.Enums;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Utilities.Extensions;

namespace FullPotential.Api.Items.Base
{
    [System.Serializable]
    public abstract class SpellOrGadgetItemBase : ItemWithTargetingAndShapeBase
    {
        public const string AliasSegmentSog = "sog";

        public ResourceConsumptionType ResourceConsumptionType { get; protected set; }

        public int ChargePercentage { get; set; }

        public int GetResourceCost()
        {
            //todo: scale up with number of effects

            var returnValue = GetHighInLowOutInRange(Attributes.Efficiency, 5, 50);
            //Debug.Log("GetResourceCost: " + returnValue);
            return (int)returnValue;
        }

        public float GetContinuousRange()
        {
            var returnValue = Attributes.Range / 100f * 10;
            //Debug.Log("GetContinuousRange: " + returnValue);
            return returnValue;
        }

        public float GetProjectileSpeed()
        {
            var castSpeed = Attributes.Speed / 50f;
            var returnValue = castSpeed < 0.5
                ? 0.5f
                : castSpeed;
            //Debug.Log("GetProjectileSpeed: " + returnValue);
            return returnValue;
        }

        public float GetChargeTime()
        {
            var returnValue = GetHighInLowOutInRange(Attributes.Speed, 0, 2);
            //Debug.Log("GetSogChargeTime: " + returnValue);
            return returnValue;
        }

        public float GetCooldownTime()
        {
            //todo: cooldown or charge but not both
            //var returnValue = GetValueInRangeHighLow(Attributes.Recovery, 0, 2);
            ////Debug.Log("GetSogChargeTime: " + returnValue);
            //return returnValue;
            return 0;
        }

        public float GetEffectDuration()
        {
            var returnValue = Attributes.Duration / 10f;
            //Debug.Log("GetDuration: " + returnValue);
            return returnValue;
        }

        public float GetDps()
        {
            var itemDamage = _valueCalculator.GetDamageValueFromAttack(this, 0, false);

            var healthEffects = Effects
                .Where(e => e is IStatEffect se && se.StatToAffect == AffectableStat.Health)
                .Select(e => (IStatEffect)e)
                .ToList();
            
            var single = healthEffects.Where(se => se.Affect == Affect.SingleDecrease);
            var singleDamage = single.Count() * itemDamage;

            var chargeTime = GetChargeTime();
            var effectDuration = GetEffectDuration();
            var timeBetweenEffects = GetEffectTimeBetween();
            var maxNumberOfTimes = effectDuration / timeBetweenEffects;
            var periodicDamageEach = itemDamage * maxNumberOfTimes / (maxNumberOfTimes / timeBetweenEffects + chargeTime);

            var periodic = healthEffects.Where(se => se.Affect == Affect.PeriodicDecrease);
            var periodicDamage = periodic.Count() * periodicDamageEach;

            return singleDamage + periodicDamage;
            //return damage * ammoMax / (ammoMax / bulletsPerSecond + GetReloadTime());
        }

        public override string GetDescription(ILocalizer localizer, LevelOfDetail levelOfDetail = LevelOfDetail.Full, string itemName = null)
        {
            var sb = new StringBuilder();

            if (levelOfDetail == LevelOfDetail.Full)
            {
                sb.Append($"{localizer.Translate(TranslationType.Item, nameof(Name))}: {itemName.OrIfNullOrWhitespace(Name)}" + "\n");
                sb.Append($"{localizer.Translate(TranslationType.Item, nameof(RegistryType))}: {GetType().Name}" + "\n");
            }

            if (Targeting != null) { sb.Append($"{localizer.Translate(TranslationType.Attribute, nameof(Targeting))}: {localizer.GetTranslatedTypeName(Targeting)}\n"); }
            if (Shape != null) { sb.Append($"{localizer.Translate(TranslationType.Attribute, nameof(Shape))}: {localizer.GetTranslatedTypeName(Shape)}\n"); }

            if (Effects != null && Effects.Count > 0)
            {
                var localisedEffects = Effects.Select(localizer.GetTranslatedTypeName);
                sb.Append($"{localizer.Translate(TranslationType.Attribute, nameof(Effects))}: {string.Join(", ", localisedEffects)}\n");
            }

            AppendToDescription(sb, localizer, Attributes.IsSoulbound, nameof(Attributes.IsSoulbound));

            AppendToDescription(
                sb,
                localizer,
                Attributes.Efficiency,
                nameof(Attributes.Efficiency),
                AliasSegmentSog,
                RoundFloatForDisplay(GetResourceCost()),
                UnitsType.Time);

            AppendToDescription(
                sb,
                localizer,
                Attributes.Range,
                nameof(Attributes.Range),
                AliasSegmentItem,
                RoundFloatForDisplay(GetRange()));

            AppendToDescription(
                sb,
                localizer,
                Attributes.Accuracy,
                nameof(Attributes.Accuracy),
                AliasSegmentItem,
                RoundFloatForDisplay(GetAccuracy()),
                UnitsType.Percent);

            AppendToDescription(
                sb,
                localizer,
                Attributes.Speed,
                nameof(Attributes.Speed),
                AliasSegmentSog,
                RoundFloatForDisplay(GetChargeTime()),
                UnitsType.Time);

            //Commented out until I decide what to do with cooldown
            //AppendToDescription(
            //    sb,
            //    localizer,
            //    Attributes.Recovery,
            //    nameof(Attributes.Recovery),
            //    AliasSegmentSog,
            //    RoundFloatForDisplay(GetCooldownTime()),
            //    UnitsType.Time);

            AppendToDescription(
                sb,
                localizer,
                Attributes.Duration,
                nameof(Attributes.Duration),
                AliasSegmentSog,
                RoundFloatForDisplay(GetEffectDuration()),
                UnitsType.Time);

            AppendToDescription(
                sb,
                localizer,
                Attributes.Strength,
                nameof(Attributes.Strength),
                AliasSegmentItem,
                RoundFloatForDisplay(GetDps()),
                UnitsType.UnitPerTime);

            return sb.ToString();
        }

    }
}
