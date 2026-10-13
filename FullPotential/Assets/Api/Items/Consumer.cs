using System.Collections.Generic;
using System.Linq;
using System.Text;

using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Gameplay;
using FullPotential.Api.Gameplay.Attributes;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Localization;
using FullPotential.Api.Logging;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Registry.Gameplay;
using FullPotential.Api.Registry.Shapes;
using FullPotential.Api.Registry.Targeting;
using FullPotential.Api.Ui;
using FullPotential.Api.Unity.Extensions;
using FullPotential.Api.Utilities.Extensions;

using UnityEngine;

namespace FullPotential.Api.Items
{
    public class Consumer : CombatItemBase, IHasTargetingAndShape, IResourceConsumer, IHasCharge, IHasItemVisuals
    {
        public const string AliasSegmentConsumer = "consumer";

        private readonly IAuditor _logger;
        private readonly ITypeRegistry _typeRegistry;

        public bool IsChargePercentageUsed => true;

        public int ChargePercentage { get; set; }

        public List<IStoppable> Stoppables { get; } = new List<IStoppable>();

        public ITargetingType TargetingType { get; set; }
        public IItemVisuals TargetingVisualsType { get; set; }
        public IItemVisuals ShapeVisualsType { get; set; }
        public IShapeType ShapeType { get; set; }
        public IResourceType ResourceType { get; set; }

        public string VisualsTypeId { get; set; }

        public IItemVisuals Visuals { get; set; }

        public Consumer(IAuditorFactory auditorFactory, ITypeRegistry typeRegistry)
        {
            _logger = auditorFactory.Create(this);
            _typeRegistry = typeRegistry;
        }

        public float GetProjectileSpeed()
        {
            var castSpeed = Attributes.GetIntValue(Speed.TypeId) / 50f;
            var returnValue = castSpeed < 0.5
                ? 0.5f
                : castSpeed;
            _logger.Debug("GetProjectileSpeed: " + returnValue);
            return returnValue;
        }

        public float GetResourceChangePerSecond()
        {
            var healthEffects = EffectTypes
                .Where(e => e is IResourceEffectType se && se.ResourceTypeIdString == ResourceTypeIds.HealthId)
                .Select(e => (IResourceEffectType)e)
                .ToList();

            var resourceChange = GetSingleEffectValueChange();

            var singleEffects = healthEffects.Where(se => se.EffectActionType is EffectActionType.SingleDecrease or EffectActionType.SingleIncrease);
            var singleChangeSum = singleEffects.Sum(_ => resourceChange);

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
                sb.Append($"{localizer.Translate(TranslationType.Item, nameof(RegistryType))}: {localizer.Translate(RegistryType)}" + "\n");
            }

            if (TargetingType != null)
            {
                sb.Append($"{localizer.Translate(TranslationType.Attribute, nameof(TargetingType))}: {localizer.Translate(TargetingType)}\n");
            }

            if (ShapeType != null)
            {
                sb.Append($"{localizer.Translate(TranslationType.Attribute, nameof(ShapeType))}: {localizer.Translate(ShapeType)}\n");
            }

            if (EffectTypes != null && EffectTypes.Count > 0)
            {
                var localisedEffects = EffectTypes.Select(localizer.Translate);
                sb.Append($"{localizer.Translate(TranslationType.Attribute, nameof(EffectTypes))}: {string.Join(", ", localisedEffects)}\n");
            }

            var resourceConsumptionType = localizer.Translate(ResourceType);

            AppendToDescription(
                sb,
                localizer,
                Attributes.GetIntValue(Efficiency.TypeId),
                nameof(Efficiency),
                AliasSegmentConsumer,
                $"{GetResourceCost()} {resourceConsumptionType}");

            AppendToDescription(
                sb,
                localizer,
                Attributes.GetIntValue(Range.TypeId),
                nameof(Range),
                AliasSegmentItem,
                localizer.Translate(GetRangeForDisplay()),
                UnitsType.Distance);

            AppendToDescription(
                sb,
                localizer,
                Attributes.GetIntValue(Accuracy.TypeId),
                nameof(Accuracy),
                AliasSegmentItem,
                Attributes.GetIntValue(Accuracy.TypeId).ToString(),
                UnitsType.Percent);

            AppendToDescription(
                sb,
                localizer,
                Attributes.GetIntValue(Speed.TypeId),
                nameof(Speed),
                AliasSegmentConsumer,
                localizer.Translate(GetChargeUpTime()),
                UnitsType.Time);

            AppendToDescription(
                sb,
                localizer,
                Attributes.GetIntValue(Recovery.TypeId),
                nameof(Recovery),
                AliasSegmentConsumer,
                localizer.Translate(GetCooldownTime()),
                UnitsType.Time);

            AppendToDescription(
                sb,
                localizer,
                Attributes.GetIntValue(Duration.TypeId),
                nameof(Duration),
                AliasSegmentConsumer,
                localizer.Translate(GetEffectDuration()),
                UnitsType.Time);

            var changePerSecond = GetResourceChangePerSecond();
            if (changePerSecond != 0)
            {
                AppendToDescription(
                    sb,
                    localizer,
                    Attributes.GetIntValue(Strength.TypeId),
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
            var returnValue = (Attributes.GetIntValue(Range.TypeId) / 100f * 15) + 5;
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

        public void InstantiateItemVisuals(IPlayerFighter playerFighter, string slotId)
        {
            if (slotId is not HandSlotIds.LeftHand or HandSlotIds.RightHand)
            {
                return;
            }

            if (ResourceType.ItemInHandDefaultPrefab == null)
            {
                _logger.Warn("No default prefab exists for resource type '{0}'", ResourceType);
                return;
            }

            _typeRegistry.LoadAddessable<GameObject>(
                ResourceType.ItemInHandDefaultPrefab,
                prefab =>
                {
                    if (prefab == null)
                    {
                        _logger.Warn("No prefab found for resource type '{0}'", ResourceType);
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
