using System;
using System.Collections.Generic;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Registry.Targeting;

namespace FullPotential.Api.Gameplay.Targeting
{
    public class Self : ITargeting
    {
        public const string Id = "b7cd2ff2-e054-4955-bde9-38b3b6b9a1bf";

        public Guid TypeId => new Guid(Id);

        public string TypeName => nameof(Self);

        public bool CanHaveShape => true;

        public bool IsContinuous => false;

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
