using System;
using System.Collections.Generic;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Registry.Targeting;

namespace FullPotential.Core.Registry.Targeting
{
    public class PointToPoint : ITargeting
    {
        public Guid TypeId => TargetingTypeIds.PointToPoint;

        public bool CanHaveShape => false;

        public bool IsContinuous => true;

        public string NetworkPrefabAddress => "Core/Prefabs/Targeting/PointToPoint.prefab";

        public string VisualsFallbackPrefabAddress => "Core/Prefabs/Targeting/BeamVisuals.prefab";

        public IEnumerable<ViableTarget> GetTargets(IFighter sourceFighter, Consumer consumer)
        {
            return null;
        }
    }
}
