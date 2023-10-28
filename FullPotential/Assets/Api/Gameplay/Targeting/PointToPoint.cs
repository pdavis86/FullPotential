using System;
using System.Collections.Generic;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Registry.Targeting;

namespace FullPotential.Api.Gameplay.Targeting
{
    public class PointToPoint : ITargeting
    {
        public Guid TypeId => new Guid("3c528d09-bd4f-4211-af17-d8721975fef1");

        public string TypeName => nameof(PointToPoint);

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
