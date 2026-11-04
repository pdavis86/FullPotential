using System.Collections.Generic;
using System.Linq;
using System.Text;

using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Gameplay;
using FullPotential.Api.Gameplay.Attributes;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Gameplay.Inventory;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Localization;
using FullPotential.Api.Logging;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Registry.Gameplay;
using FullPotential.Api.Registry.Shapes;
using FullPotential.Api.Registry.Targeting;
using FullPotential.Api.Registry.Visuals;
using FullPotential.Api.Unity.Extensions;
using FullPotential.Api.Utilities.Extensions;

using UnityEngine;

namespace FullPotential.Api.Items
{
    public class Consumer : CombatItemBase, IHasTargetingAndShape, IValuePoolConsumer, IHasCharge, IHasItemVisuals
    {
        public const string AliasSegmentConsumer = "consumer";

        private readonly IAuditor _logger;
        private readonly ITypeRegistry _typeRegistry;

        public bool IsChargePercentageUsed => true;

        public int ChargePercentage { get; set; }

        public List<IStoppable> Stoppables { get; } = new List<IStoppable>();

        public ITargetingType TargetingType { get; set; }
        public IItemVisualsType TargetingVisualsType { get; set; }
        public IItemVisualsType ShapeVisualsType { get; set; }
        public IShapeType ShapeType { get; set; }
        public IValuePoolType ValuePoolType { get; set; }

        public ItemVisualsTypeId VisualsTypeId { get; set; }

        public IItemVisualsType Visuals { get; set; }

        public Consumer(IAuditorFactory auditorFactory, ITypeRegistry typeRegistry)
        {
            _logger = auditorFactory.Create(this);
            _typeRegistry = typeRegistry;
        }

        public float GetProjectileSpeed()
        {
            var castSpeed = Attributes.GetValueOrZero(Speed.Id) / 50f;
            var returnValue = castSpeed < 0.5
                ? 0.5f
                : castSpeed;
            _logger.Debug("GetProjectileSpeed: " + returnValue);
            return returnValue;
        }

        public float GetValuePoolChangePerSecond()
        {
            var healthEffects = EffectTypes
                .Where(e => e is IValuePoolEffectType se && se.ValuePoolTypeId == ValuePoolTypeIds.HealthId)
                .Select(e => (IValuePoolEffectType)e)
                .ToList();

            var valuePoolChange = GetSingleEffectValueChange();

            var singleEffects = healthEffects.Where(se => se.EffectActionType is EffectActionType.SingleDecrease or EffectActionType.SingleIncrease);
            var singleChangeSum = singleEffects.Sum(_ => valuePoolChange);

            var periodicChange = GetPeriodicEffectValueChange();

            var periodicEffects = healthEffects.Where(se => se.EffectActionType is EffectActionType.PeriodicDecrease or EffectActionType.PeriodicIncrease);
            var periodicChangeSum = periodicEffects.Sum(_ => periodicChange) * -1;

            return singleChangeSum + periodicChangeSum;
        }

        public override string GetDescription(ILocalizer localizer, LevelOfDetail levelOfDetail = LevelOfDetail.Full)
        {
            var sb = new StringBuilder();

            if (levelOfDetail == LevelOfDetail.Full)
            {
                sb.Append($"{localizer.Translate(TranslationType.Item, nameof(Name))}: {Name}" + "\n");

                if (RegistryType != null)
                {
                    sb.Append($"{localizer.Translate(TranslationType.Item, nameof(RegistryType))}: {localizer.Translate(RegistryType)}" + "\n");
                }
            }

            if (TargetingType != null)
            {
                sb.Append($"{localizer.Translate(TranslationType.Item, nameof(TargetingType))}: {localizer.Translate(TargetingType)}\n");
            }

            if (ShapeType != null)
            {
                sb.Append($"{localizer.Translate(TranslationType.Item, nameof(ShapeType))}: {localizer.Translate(ShapeType)}\n");
            }

            if (EffectTypes != null && EffectTypes.Count > 0)
            {
                var localisedEffects = EffectTypes.Select(localizer.Translate);
                sb.Append($"{localizer.Translate(TranslationType.Item, nameof(EffectTypes))}: {string.Join(", ", localisedEffects)}\n");
            }

            var valuePoolConsumptionType = localizer.Translate(ValuePoolType);

            AppendToDescription(
                sb,
                localizer,
                Attributes.GetValueOrZero(Efficiency.Id),
                nameof(Efficiency),
                AliasSegmentConsumer,
                $"{GetValuePoolCost()} {valuePoolConsumptionType}");

            AppendToDescription(
                sb,
                localizer,
                Attributes.GetValueOrZero(Range.Id),
                nameof(Range),
                AliasSegmentItem,
                localizer.Translate(GetRangeForDisplay()),
                UnitsType.Distance);

            AppendToDescription(
                sb,
                localizer,
                Attributes.GetValueOrZero(Accuracy.Id),
                nameof(Accuracy),
                AliasSegmentItem,
                Attributes.GetValueOrZero(Accuracy.Id).ToString(),
                UnitsType.Percent);

            AppendToDescription(
                sb,
                localizer,
                Attributes.GetValueOrZero(Speed.Id),
                nameof(Speed),
                AliasSegmentConsumer,
                localizer.Translate(GetChargeUpTime()),
                UnitsType.Time);

            AppendToDescription(
                sb,
                localizer,
                Attributes.GetValueOrZero(Recovery.Id),
                nameof(Recovery),
                AliasSegmentConsumer,
                localizer.Translate(GetCooldownTime()),
                UnitsType.Time);

            AppendToDescription(
                sb,
                localizer,
                Attributes.GetValueOrZero(Duration.Id),
                nameof(Duration),
                AliasSegmentConsumer,
                localizer.Translate(GetEffectDuration()),
                UnitsType.Time);

            var changePerSecond = GetValuePoolChangePerSecond();
            if (changePerSecond != 0)
            {
                AppendToDescription(
                    sb,
                    localizer,
                    Attributes.GetValueOrZero(Strength.Id),
                    nameof(Strength),
                    AliasSegmentConsumer,
                    localizer.Translate(changePerSecond),
                    UnitsType.UnitPerTime);
            }

            return sb.ToString().Trim();
        }

        public override IEnumerable<string> ValidateCraftingAttempt(List<ItemBase> components)
        {
            var errors = new List<string>();

            if (EffectTypes.Count == 0)
            {
                errors.Add("crafting.error.missingeffect");
            }

            return errors;
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
            var returnValue = (Attributes.GetValueOrZero(Range.Id) / 100f * 15) + 5;
            return returnValue;
        }

        public float GetRangeForDisplay()
        {
            //Rough translation from Unity units to metres
            return GetRange() * 0.6f;
        }

        public override string GetTextForUiComponent(string id, IPlayerFighter playerFighter)
        {
            return null;
        }

        public void InstantiateItemVisuals(IPlayerFighter playerFighter, SlotId slotId)
        {
            if (slotId != HandSlotIds.LeftHand && slotId != HandSlotIds.RightHand)
            {
                return;
            }

            if (ValuePoolType.ItemInHandDefaultPrefab == null)
            {
                _logger.Warn("No default prefab exists for valuePool type '{0}'", ValuePoolType);
                return;
            }

            _typeRegistry.LoadAddessable<GameObject>(
                ValuePoolType.ItemInHandDefaultPrefab,
                prefab =>
                {
                    if (prefab == null)
                    {
                        _logger.Warn("No prefab found for valuePool type '{0}'", ValuePoolType);
                        return;
                    }

                    var newObj = Object.Instantiate(prefab, playerFighter.InFrontOfPlayerGameObject.transform);

                    newObj.transform.localPosition = slotId == HandSlotIds.LeftHand
                        ? playerFighter.Positions.LeftHand.localPosition
                        : playerFighter.Positions.RightHand.localPosition;

                    if (playerFighter.IsLocalClient())
                    {
                        newObj.SetGameLayerRecursive(playerFighter.InFrontOfPlayerGameObject.layer);
                    }

                    playerFighter.Inventory.SetEquippedItemVisuals(slotId, newObj);
                });
        }
    }
}
