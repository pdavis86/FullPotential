using System;
using System.Collections.Generic;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Registry.Targeting;

namespace FullPotential.Api.Gameplay.Targeting
{
    //todo: Why is Projectile in API instead of Core?
    public class Projectile : ITargeting
    {
        public const string TypeIdString = "6e41729e-bb21-44f8-8fb9-b9ad48c0e680";

        public Guid TypeId => new Guid(TypeIdString);

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
