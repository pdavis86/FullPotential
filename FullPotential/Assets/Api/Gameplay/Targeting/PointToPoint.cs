using System;
using System.Collections.Generic;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Registry.Targeting;
using UnityEngine;

namespace FullPotential.Api.Gameplay.Targeting
{
    public class PointToPoint : ITargeting
    {
        public Guid TypeId => new Guid("3c528d09-bd4f-4211-af17-d8721975fef1");

        public string TypeName => nameof(PointToPoint);

        public bool CanHaveShape => false;

        public bool IsContinuous => true;

        public IEnumerable<ViableTarget> GetTargets(IFighter sourceFighter, Consumer consumer)
        {
            if (Physics.Raycast(sourceFighter.LookTransform.position, sourceFighter.LookTransform.forward, out var hit, consumer.GetRange()))
            {
                if (hit.transform.gameObject == sourceFighter.GameObject)
                {
                    Debug.LogWarning("PointToPoint lead to the source player!");
                    return null;
                }

                return new[]
                {
                    new ViableTarget { GameObject = hit.transform.gameObject, Position = hit.transform.position, EffectPercentage = 1 }
                };
            }

            return null;
        }
    }
}
