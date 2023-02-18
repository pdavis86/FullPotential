using System.Linq;
using System.Text;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Localization;
using FullPotential.Api.Localization.Enums;
using FullPotential.Api.Networking;
using FullPotential.Api.Obsolete;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Utilities.Extensions;

namespace FullPotential.Api.Items.Types
{
    [System.Serializable]
    public class Consumer : ItemWithTargetingAndShapeBase
    {
        public const string AliasSegmentConsumer = "consumer";
        public const string AliasSegmentEffects = "effects";

        public ResourceConsumptionType ResourceConsumptionType;

        public int ChargePercentage { get; set; }

        public int GetResourceCost()
        {
            //todo: scale up with number of effects

            var returnValue = GetHighInLowOutInRange(Attributes.Efficiency, 5, 50);
            //Debug.Log("GetResourceCost: " + returnValue);
            return (int)returnValue;
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
            //Debug.Log("GetChargeTime: " + returnValue);
            return returnValue;
        }

        //todo: zzz v0.4.1 - remove SoG cooldown if not necessary
        //public float GetCooldownTime()
        //{
        //    var returnValue = GetHighInLowOutInRange(Attributes.Recovery, 0, 2);
        //    //Debug.Log("GetSogChargeTime: " + returnValue);
        //    return returnValue;
        //}

        public float GetDps()
        {
            var itemDamage = _effectService.GetDamageValueFromAttack(this, 0, false);

            var healthEffects = Effects
                .Where(e => e is IStatEffect se && se.StatToAffect == AffectableStat.Health)
                .Select(e => (IStatEffect)e)
                .ToList();
            
            var single = healthEffects.Where(se => se.AffectType == AffectType.SingleDecrease || se.AffectType == AffectType.SingleIncrease);
            var singleDamage = single.Count() * itemDamage;

            var periodic = healthEffects.Where(se => se.AffectType == AffectType.PeriodicDecrease || se.AffectType == AffectType.PeriodicIncrease);
            var periodicDamage = periodic.Sum(GetPeriodicStatDamagePerSecond) * -1;

            return singleDamage + periodicDamage;
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
                AliasSegmentConsumer,
                RoundFloatForDisplay(GetResourceCost()));

            AppendToDescription(
                sb,
                localizer,
                Attributes.Range,
                nameof(Attributes.Range),
                AliasSegmentItem,
                RoundFloatForDisplay(GetRangeForDisplay()));

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
                AliasSegmentConsumer,
                RoundFloatForDisplay(GetChargeTime()),
                UnitsType.Time);

            //todo: zzz v0.4.1 - Commented out until I decide what to do with cooldown
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
                AliasSegmentConsumer,
                RoundFloatForDisplay(GetEffectDuration()),
                UnitsType.Time);

            AppendToDescription(
                sb,
                localizer,
                Attributes.Speed,
                nameof(Attributes.Speed),
                AliasSegmentEffects,
                RoundFloatForDisplay(GetEffectTimeBetween()),
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
