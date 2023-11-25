using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FullPotential.Api.Gameplay;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Localization;
using FullPotential.Api.Localization.Enums;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Registry.Resources;
using FullPotential.Api.Utilities.Extensions;

namespace FullPotential.Api.Items.Types
{
    [Serializable]
    public class Consumer : ItemWithTargetingAndShapeBase, IResourceConsumer
    {
        public const string AliasSegmentConsumer = "consumer";
        public const string AliasSegmentEffects = "effects";

        private IResource _resourceType;

        public string ResourceTypeId;

        public IResource ResourceType
        {
            get => _resourceType;
            set
            {
                _resourceType = value;
                ResourceTypeId = _resourceType?.TypeId.ToString();
            }
        }

        public int ChargePercentage { get; set; }

        public List<IStoppable> Stoppables { get; } = new List<IStoppable>();

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
            var itemDamage = GetCombatService().GetDamageValueFromAttack(this, 0, false);

            var healthEffects = Effects
                .Where(e => e is IResourceEffect se && se.ResourceTypeId == ResourceTypeIds.Health)
                .Select(e => (IResourceEffect)e)
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

            if (Targeting != null)
            {
                sb.Append($"{localizer.Translate(TranslationType.Attribute, nameof(Targeting))}: {localizer.Translate(Targeting)}\n");
            }

            if (Shape != null)
            {
                sb.Append($"{localizer.Translate(TranslationType.Attribute, nameof(Shape))}: {localizer.Translate(Shape)}\n");
            }

            if (Effects != null && Effects.Count > 0)
            {
                var localisedEffects = Effects.Select(localizer.Translate);
                sb.Append($"{localizer.Translate(TranslationType.Attribute, nameof(Effects))}: {string.Join(", ", localisedEffects)}\n");
            }

            AppendToDescription(sb, localizer, Attributes.IsSoulbound, nameof(Attributes.IsSoulbound));

            var resourceConsumptionType = localizer.Translate(ResourceType);

            AppendToDescription(
                sb,
                localizer,
                Attributes.Efficiency,
                nameof(Attributes.Efficiency),
                AliasSegmentConsumer,
                $"{GetResourceCost()} {resourceConsumptionType}");

            AppendToDescription(
                sb,
                localizer,
                Attributes.Range,
                nameof(Attributes.Range),
                AliasSegmentItem,
                localizer.TranslateFloat(GetRangeForDisplay()));

            AppendToDescription(
                sb,
                localizer,
                Attributes.Accuracy,
                nameof(Attributes.Accuracy),
                AliasSegmentItem,
                localizer.TranslateFloat(GetAccuracy()),
                UnitsType.Percent);

            AppendToDescription(
                sb,
                localizer,
                Attributes.Speed,
                nameof(Attributes.Speed),
                AliasSegmentConsumer,
                localizer.TranslateFloat(GetChargeTime()),
                UnitsType.Time);

            //todo: zzz v0.4.1 - Commented out until I decide what to do with cooldown
            //AppendToDescription(
            //    sb,
            //    localizer,
            //    Attributes.Recovery,
            //    nameof(Attributes.Recovery),
            //    AliasSegmentSog,
            //    localizer.TranslateFloat(GetCooldownTime()),
            //    UnitsType.Time);

            AppendToDescription(
                sb,
                localizer,
                Attributes.Duration,
                nameof(Attributes.Duration),
                AliasSegmentConsumer,
                localizer.TranslateFloat(GetEffectDuration()),
                UnitsType.Time);

            AppendToDescription(
                sb,
                localizer,
                Attributes.Speed,
                nameof(Attributes.Speed),
                AliasSegmentEffects,
                localizer.TranslateFloat(GetEffectTimeBetween()),
                UnitsType.Time);

            AppendToDescription(
                sb,
                localizer,
                Attributes.Strength,
                nameof(Attributes.Strength),
                AliasSegmentItem,
                localizer.TranslateFloat(GetDps()),
                UnitsType.UnitPerTime);

            return sb.ToString().Trim();
        }

        public void StopStoppables()
        {
            foreach (var stoppable in Stoppables)
            {
                stoppable.Stop();
            }

            Stoppables.Clear();
        }

    }
}
