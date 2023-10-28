using System;
using System.Collections.Generic;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Registry.Targeting;
using UnityEngine;

namespace FullPotential.Api.Gameplay.Targeting
{
    public class Touch : ITargeting
    {
        public Guid TypeId => new Guid("144cc142-2e64-476f-b3a6-de57cc3abd05");

        public string TypeName => nameof(Touch);

        public bool CanHaveShape => false;

        public bool IsContinuous => false;

        public string NetworkPrefabAddress => null;

        public string VisualsFallbackPrefabAddress => null;

        public IEnumerable<ViableTarget> GetTargets(IFighter sourceFighter, Consumer consumer)
        {
            const int maxDistance = 3;

            if (Physics.Raycast(sourceFighter.LookTransform.position, sourceFighter.LookTransform.forward, out var hit, maxDistance))
            {
                return new[]
                {
                    new ViableTarget { GameObject = hit.transform.gameObject, Position = hit.transform.position, EffectPercentage = 1 }
                };
            }

            return null;
        }
    }
}
