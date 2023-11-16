using System;
using System.Collections.Generic;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Registry.Targeting;

namespace FullPotential.Core.Gameplay.Targeting
{
    public class Projectile : ITargeting
    {
        public Guid TypeId => TargetingTypeIds.Projectile;

        public string TypeName => nameof(Projectile);

        public bool CanHaveShape => true;

        public bool IsContinuous => false;

        public string NetworkPrefabAddress => "Core/Prefabs/Targeting/Projectile.prefab";

        public string VisualsFallbackPrefabAddress => "Core/Prefabs/Targeting/ProjectileVisuals.prefab";

        public IEnumerable<ViableTarget> GetTargets(IFighter sourceFighter, Consumer consumer)
        {
            return null;
        }
    }
}
