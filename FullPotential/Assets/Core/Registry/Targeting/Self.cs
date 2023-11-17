using System;
using System.Collections.Generic;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Registry.Targeting;

namespace FullPotential.Core.Registry.Targeting
{
    public class Self : ITargeting
    {
        public Guid TypeId => TargetingTypeIds.Self;

        public string TypeName => nameof(Self);

        public bool CanHaveShape => true;

        public bool IsContinuous => false;

        public string NetworkPrefabAddress => null;

        public string VisualsFallbackPrefabAddress => null;

        public IEnumerable<ViableTarget> GetTargets(IFighter sourceFighter, Consumer consumer)
        {
            return new[]
            {
                new ViableTarget { GameObject = sourceFighter.GameObject, Position = sourceFighter.Transform.position, EffectPercentage = 1 }
            };
        }
    }
}
