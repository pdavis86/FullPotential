using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Logging;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Core.Gameplay.Combat
{
    public class AttributeEffectHandler : ICombatEffectHandler
    {
        private readonly IAuditor _logger;
        private readonly ICombatService _combatService;

        public AttributeEffectHandler(IAuditorFactory auditorFactory, ICombatService combatService)
        {
            _logger = auditorFactory.Create(this);
            _combatService = combatService;
        }

        public bool CanHandle(IEffectType effect) => effect is IAttributeEffect;

        public void Apply(CombatEffectContext context, IEffectType effect)
        {
            if (!context.Target.TryGetComponent<FighterBase>(out var targetFighter))
            {
                _logger.Debug("Not applying effect {0} to {1} because they are not an FighterBase", effect.TypeId, context.Target.name);
                return;
            }

            _logger.Debug("Applying effect {0} to {1}", effect.TypeId, targetFighter.FighterName);
            var attributeEffect = (IAttributeEffect)effect;
            var result = _combatService.GetCombatResult(context.SourceFighter, context.ItemUsed, effect, targetFighter);
            var duration = context.ItemUsed?.GetEffectDuration() ?? 0;
            targetFighter.AddAttributeModifier(attributeEffect, result.Change, duration);
        }
    }
}
