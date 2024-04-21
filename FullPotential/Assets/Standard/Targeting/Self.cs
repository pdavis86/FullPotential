using System;
using System.Collections.Generic;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Registry.Targeting;

namespace FullPotential.Standard.Targeting
{
    public class Self : ITargetingType
    {
        public const string TypeIdString = "b7cd2ff2-e054-4955-bde9-38b3b6b9a1bf";

        private static readonly Guid Id = new Guid(TypeIdString);

        public Guid TypeId => Id;

        public bool CanHaveShape => true;

        public bool IsContinuous => false;

        public string NetworkPrefabAddress => null;

        public IEnumerable<ViableTarget> GetTargets(FighterBase sourceFighter, Consumer consumer)
        {
            return new[]
            {
                new ViableTarget { GameObject = sourceFighter.GameObject, Position = sourceFighter.Transform.position }
            };
        }
    }
}
