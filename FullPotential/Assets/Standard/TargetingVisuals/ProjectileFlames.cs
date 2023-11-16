using System;
using FullPotential.Api.Registry.Targeting;

namespace FullPotential.Standard.TargetingVisuals
{
    public class ProjectileFlames : ITargetingVisuals
    {
        public Guid TypeId => new Guid("5f901588-e3cc-442d-9330-7897f198b211");

        public string TypeName => nameof(ProjectileFlames);

        public string PrefabAddress => "Standard/Prefabs/Targeting/Projectile.prefab";

        public Guid ApplicableToTypeId => TargetingTypeIds.Projectile;
    }
}
