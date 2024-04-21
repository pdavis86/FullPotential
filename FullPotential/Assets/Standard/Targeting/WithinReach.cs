using System;
using System.Collections.Generic;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Registry.Targeting;
using UnityEngine;

namespace FullPotential.Standard.Targeting
{
    public class WithinReach : ITargetingType
    {
        public const string TypeIdString = "144cc142-2e64-476f-b3a6-de57cc3abd05";

        private static readonly Guid Id = new Guid(TypeIdString);

        public Guid TypeId => Id;

        public bool CanHaveShape => false;

        public bool IsContinuous => false;

        public string NetworkPrefabAddress => null;

        public IEnumerable<ViableTarget> GetTargets(FighterBase sourceFighter, Consumer consumer)
        {
            const int maxDistance = 3;

            if (Physics.Raycast(sourceFighter.LookTransform.position, sourceFighter.LookTransform.forward, out var hit, maxDistance))
            {
                return new[]
                {
                    new ViableTarget { GameObject = hit.transform.gameObject, Position = hit.transform.position }
                };
            }

            return null;
        }
    }
}
