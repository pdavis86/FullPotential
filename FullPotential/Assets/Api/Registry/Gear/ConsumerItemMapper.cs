using System;
using System.Collections.Generic;
using System.Linq;

using FullPotential.Api.Gameplay.Attributes;
using FullPotential.Api.Gameplay.Crafting;
using FullPotential.Api.Gameplay.Inventory;
using FullPotential.Api.Ioc;
using FullPotential.Api.Items;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Localization;
using FullPotential.Api.Registry.Gameplay;
using FullPotential.Api.Utilities.Extensions;

using Range = FullPotential.Api.Gameplay.Attributes.Range;

namespace FullPotential.Api.Registry.Gear
{
    public class ConsumerItemMapper : IItemTypeMapping
    {
        private readonly ITypeRegistry _typeRegistry;
        private readonly ILocalizer _localizer;

        public Type GroupType => typeof(IConsumerType);

        public ConsumerItemMapper(
            ITypeRegistry typeRegistry,
            ILocalizer localizer)
        {
            _typeRegistry = typeRegistry;
            _localizer = localizer;
        }

        public ItemBase CreateNew()
        {
            return DependenciesContext.Dependencies.CreateInstance<Consumer>();
        }

        public ItemBase GetItemFromCrafting(IList<ItemBase> components, string itemTypeId, string itemName, Dictionary<string, string> properties)
        {
            // todo: zzz v0.6 - Cannot inject IResultFactory as it is not ready to be used yet
            var resultFactory = DependenciesContext.Dependencies.GetService<IResultFactory>();

            var consumer = DependenciesContext.Dependencies.CreateInstance<Consumer>();

            var valuePoolTypeId = properties.ContainsKey(IValuePoolConsumer.PropertyKeyResourceTypeId)
                   ? properties[IValuePoolConsumer.PropertyKeyResourceTypeId]
                   : properties.GetStringValueOrNull(IValuePoolConsumer.PropertyKeyValuePoolTypeId);

            consumer.ValuePoolType = _typeRegistry.GetRegisteredByTypeId<IValuePoolType>(valuePoolTypeId);

            consumer.Attributes = new Dictionary<AttributeTypeId, int>
            {
                { Strength.Id, resultFactory.ComputeAttribute(components, Strength.Id) },
                { Efficiency.Id, resultFactory.ComputeAttribute(components, Efficiency.Id) },
                { Range.Id, resultFactory.ComputeAttribute(components, Range.Id) },
                { Accuracy.Id, resultFactory.ComputeAttribute(components, Accuracy.Id) },
                { Speed.Id, resultFactory.ComputeAttribute(components, Speed.Id) },
                { Recovery.Id, resultFactory.ComputeAttribute(components, Recovery.Id) },
                { Duration.Id, resultFactory.ComputeAttribute(components, Duration.Id) },
            };

            consumer.Properties = properties;

            resultFactory.SetTargetingAndShape(consumer, components);
            resultFactory.SetEffects(consumer, components);

            if (!itemName.IsNullOrWhiteSpace())
            {
                consumer.Name = itemName;
            }
            else
            {
                var suffix = _localizer.Translate(consumer.TargetingType)
                    + (consumer.ShapeType != null ? " " + _localizer.Translate(consumer.ShapeType) : null)
                    + " " + _localizer.Translate(TranslationType.ItemType, nameof(IConsumerType));

                if (consumer.EffectTypes.Count > 0)
                {
                    consumer.Name = _localizer.Translate(consumer.EffectTypes.First()) + " " + suffix;
                }
                else
                {
                    resultFactory.SetItemName(consumer, true, suffix);
                }
            }

            return consumer;
        }
    }
}
