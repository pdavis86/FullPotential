using System;
using System.Collections.Generic;
using System.Linq;

using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.EventManagement;
using FullPotential.Api.Gameplay.Combat.Events;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Gameplay.Inventory;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Registry.Gameplay;
using FullPotential.Api.Utilities;

namespace FullPotential.Api.Gameplay.Behaviours
{
    // todo: can ActiveEffectManager be moved to Core?
    internal sealed class ActiveEffectManager
    {
        private readonly LivingEntityBase _livingEntity;
        private readonly IClock _clock;
        private readonly IEventBus _eventBus;
        private readonly Action<IEffectType, int, DateTime> _replicateEffect;
        private readonly Action<ValuePoolTypeId> _updateValuePool;
        private readonly List<ActiveEffect> _activeEffects = new List<ActiveEffect>();

        public ActiveEffectManager(
            LivingEntityBase livingEntity,
            IClock clock,
            IEventBus eventBus,
            Action<IEffectType, int, DateTime> replicateEffect,
            Action<ValuePoolTypeId> updateValuePool)
        {
            _livingEntity = livingEntity;
            _clock = clock;
            _eventBus = eventBus;
            _replicateEffect = replicateEffect;
            _updateValuePool = updateValuePool;
        }

        public List<ActiveEffect> GetActiveEffects(bool isAlive)
        {
            return isAlive ? _activeEffects : new List<ActiveEffect>();
        }

        public int GetAttributeAdjustment(AttributeTypeId attributeId)
        {
            return _activeEffects
                .Where(x => x.Effect is IAttributeEffect attributeEffect && attributeEffect.AttributeId == attributeId)
                .Sum(x => x.Change * (x.Effect is IAttributeEffect attributeEffect && attributeEffect.IsTemporaryMaxIncrease ? 1 : -1));
        }

        public int GetValuePoolMaxAdjustment(ValuePoolTypeId valuePoolTypeId)
        {
            return _activeEffects
                .Where(x =>
                    x.Effect is IValuePoolEffectType valuePoolEffect
                    && valuePoolEffect.ValuePoolTypeId == valuePoolTypeId
                    && valuePoolEffect.EffectActionType is EffectActionType.TemporaryMaxIncrease or EffectActionType.TemporaryMaxDecrease)
                .Sum(x => x.Change);
        }

        public void AddOrUpdate(IEffectType effect, int change, DateTime expiry, long timerStartTimestamp, double durationSeconds)
        {
            var valuePoolEffect = effect as IValuePoolEffectType;

            //todo: zzz v0.7 - Add health single decrease effect to a UI options list that can be changed
            var hideEffectFromFighter = valuePoolEffect != null
                && valuePoolEffect.EffectActionType == EffectActionType.SingleDecrease
                && valuePoolEffect.ValuePoolTypeId == ValuePoolTypeIds.HealthId;

            if (hideEffectFromFighter)
            {
                return;
            }

            var showExpiry = !(valuePoolEffect != null
                && valuePoolEffect.EffectActionType is EffectActionType.SingleDecrease or EffectActionType.SingleIncrease);

            var effectMatch = _activeEffects.FirstOrDefault(x => x.Effect == effect);
            if (effectMatch != null)
            {
                var multipleAllowed =
                    (valuePoolEffect != null && DoesActionTypeAllowMultiple(valuePoolEffect.EffectActionType))
                    || effect is IAttributeEffect;

                if (multipleAllowed)
                {
                    var anotherActiveEffect = CreateActiveEffect(effect, change, expiry, timerStartTimestamp, durationSeconds, showExpiry);
                    _activeEffects.Add(anotherActiveEffect);
                    _eventBus.Publish(new ActiveEffectAddedEvent(_livingEntity, anotherActiveEffect));
                }
                else
                {
                    effectMatch.Change = change;
                    effectMatch.Expiry = expiry;
                    effectMatch.TimerStartTimestamp = timerStartTimestamp;
                    effectMatch.DurationSeconds = durationSeconds;
                    _eventBus.Publish(new ActiveEffectUpdatedEvent(_livingEntity, effectMatch));
                }
            }
            else
            {
                var newActiveEffect = CreateActiveEffect(effect, change, expiry, timerStartTimestamp, durationSeconds, showExpiry);
                _activeEffects.Add(newActiveEffect);
                _eventBus.Publish(new ActiveEffectAddedEvent(_livingEntity, newActiveEffect));
            }

            _replicateEffect(effect, change, expiry);
        }

        public void RemoveExpiredEffects()
        {
            //Note: .ToList() needed to avoid the "Collection was modified" exception
            var expiredEffects = _activeEffects
                .Where(x => _clock.GetElapsedSeconds(x.TimerStartTimestamp) >= x.DurationSeconds)
                .ToList();

            var valuePoolsNeedingUpdates = expiredEffects
                .Where(x =>
                    x.Effect is IValuePoolEffectType valuePoolEffect
                    && valuePoolEffect.EffectActionType is EffectActionType.TemporaryMaxIncrease or EffectActionType.TemporaryMaxDecrease)
                .Select(x => ((IValuePoolEffectType)x.Effect).ValuePoolTypeId)
                .Distinct()
                .ToList();

            var anyEffectRemoved = false;
            foreach (var activeEffect in expiredEffects)
            {
                anyEffectRemoved |= _activeEffects.Remove(activeEffect);
            }

            if (anyEffectRemoved)
            {
                _eventBus.Publish(new ActiveEffectUpdatedEvent(_livingEntity, null));
            }

            foreach (var valuePoolTypeId in valuePoolsNeedingUpdates)
            {
                _updateValuePool(valuePoolTypeId);
            }
        }

        public void Clear()
        {
            _activeEffects.Clear();
        }

        private static ActiveEffect CreateActiveEffect(
            IEffectType effect,
            int change,
            DateTime expiry,
            long timerStartTimestamp,
            double durationSeconds,
            bool showExpiry)
        {
            return new ActiveEffect
            {
                Id = Guid.NewGuid(),
                Effect = effect,
                Change = change,
                Expiry = expiry,
                TimerStartTimestamp = timerStartTimestamp,
                DurationSeconds = durationSeconds,
                ShowExpiry = showExpiry
            };
        }

        private static bool DoesActionTypeAllowMultiple(EffectActionType effectActionType)
        {
            return effectActionType is EffectActionType.TemporaryMaxIncrease or EffectActionType.TemporaryMaxDecrease;
        }
    }
}
