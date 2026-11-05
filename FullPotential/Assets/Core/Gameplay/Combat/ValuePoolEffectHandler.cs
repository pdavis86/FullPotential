using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Logging;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Core.Gameplay.Combat
{
    public class ValuePoolEffectHandler : ICombatEffectHandler
    {
        private readonly IAuditor _logger;

        public ValuePoolEffectHandler(IAuditorFactory auditorFactory)
        {
            _logger = auditorFactory.Create(this);
        }

        public bool CanHandle(IEffectType effect) => effect is IValuePoolEffectType;

        public void Apply(CombatEffectContext context, IEffectType effect)
        {
            if (!context.Target.TryGetComponent<FighterBase>(out var targetFighter))
            {
                _logger.Debug("Not applying effect {0} to {1} because they are not an FighterBase", effect.TypeId, context.Target.name);
                return;
            }

            var valuePoolEffect = (IValuePoolEffectType)effect;
            switch (valuePoolEffect.EffectActionType)
            {
                case EffectActionType.PeriodicDecrease:
                case EffectActionType.PeriodicIncrease:
                    targetFighter.ApplyPeriodicActionToValuePool(context.SourceFighter, context.ItemUsed, valuePoolEffect, context.Position);
                    return;
                case EffectActionType.SingleDecrease:
                case EffectActionType.SingleIncrease:
                    targetFighter.ApplySingleValueChangeToValuePool(context.SourceFighter, context.ItemUsed, valuePoolEffect, context.Position);
                    return;
                case EffectActionType.TemporaryMaxDecrease:
                case EffectActionType.TemporaryMaxIncrease:
                    targetFighter.ApplyTemporaryMaxActionToValuePool(context.SourceFighter, context.ItemUsed, valuePoolEffect);
                    return;
                default:
                    _logger.Error("Not implemented handling for affect type {0}", valuePoolEffect.EffectActionType);
                    return;
            }
        }
    }
}
