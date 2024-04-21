using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Gameplay;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Localization;
using FullPotential.Api.Localization.Enums;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Registry.Gameplay;
using FullPotential.Api.Utilities.Extensions;

namespace FullPotential.Api.Items.Types
{
    [Serializable]
    public class Consumer : ItemWithTargetingAndShapeBase, IResourceConsumer, IHasCharge
    {
        public const string AliasSegmentConsumer = "consumer";

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

        public bool IsChargePercentageUsed => true;

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

        public float GetResourceChangePerSecond()
        {
            var healthEffects = Effects
                .Where(e => e is IResourceEffect se && se.ResourceTypeIdString == ResourceTypeIds.HealthId)
                .Select(e => (IResourceEffect)e)
                .ToList();

            var resourceChange = GetSingleEffectValueChange();

            var singleEffects = healthEffects.Where(se => se.EffectActionType == EffectActionType.SingleDecrease || se.EffectActionType == EffectActionType.SingleIncrease);
            var singleChangeSum = singleEffects.Sum(_ => resourceChange);

            var periodicChange = GetPeriodicEffectValueChange();

            var periodicEffects = healthEffects.Where(se => se.EffectActionType == EffectActionType.PeriodicDecrease || se.EffectActionType == EffectActionType.PeriodicIncrease);
            var periodicChangeSum = periodicEffects.Sum(_ => periodicChange) * -1;

            return singleChangeSum + periodicChangeSum;
        }

        public override string GetDescription(ILocalizer localizer, LevelOfDetail levelOfDetail = LevelOfDetail.Full, string itemName = null)
        {
            var sb = new StringBuilder();

            if (levelOfDetail == LevelOfDetail.Full)
            {
                sb.Append($"{localizer.Translate(TranslationType.Item, nameof(Name))}: {itemName.OrIfNullOrWhitespace(Name)}" + "\n");
                sb.Append($"{localizer.Translate(TranslationType.Item, nameof(RegistryType))}: {localizer.Translate(TranslationType.ItemType, GetType().Name)}" + "\n");
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
                localizer.TranslateFloat(GetRangeForDisplay()),
                UnitsType.Distance);

            AppendToDescription(
                sb,
                localizer,
                Attributes.Accuracy,
                nameof(Attributes.Accuracy),
                AliasSegmentItem,
                localizer.TranslateFloat(Attributes.Accuracy),
                UnitsType.Percent);

            AppendToDescription(
                sb,
                localizer,
                Attributes.Speed,
                nameof(Attributes.Speed),
                AliasSegmentConsumer,
                localizer.TranslateFloat(GetChargeUpTime()),
                UnitsType.Time);

            AppendToDescription(
                sb,
                localizer,
                Attributes.Recovery,
                nameof(Attributes.Recovery),
                AliasSegmentConsumer,
                localizer.TranslateFloat(GetCooldownTime()),
                UnitsType.Time);

            AppendToDescription(
                sb,
                localizer,
                Attributes.Duration,
                nameof(Attributes.Duration),
                AliasSegmentConsumer,
                localizer.TranslateFloat(GetEffectDuration()),
                UnitsType.Time);

            var changePerSecond = GetResourceChangePerSecond();
            if (changePerSecond != 0)
            {
                AppendToDescription(
                    sb,
                    localizer,
                    Attributes.Strength,
                    nameof(Attributes.Strength),
                    AliasSegmentConsumer,
                    localizer.TranslateFloat(changePerSecond),
                    UnitsType.UnitPerTime);
            }

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

        public float GetRange()
        {
            var returnValue = Attributes.Range / 100f * 15 + 5;
            return returnValue;
        }

        public float GetRangeForDisplay()
        {
            //Rough translation from Unity units to metres
            return GetRange() * 0.6f;
        }
    }
}
