using System;
using FullPotential.Api.Registry.Targeting;
using FullPotential.Standard.Targeting;

namespace FullPotential.Standard.TargetingVisuals
{
    public class ProjectileFlames : ITargetingVisuals
    {
        public Guid TypeId => new Guid("5f901588-e3cc-442d-9330-7897f198b211");

        public string PrefabAddress => "Standard/Prefabs/Targeting/ProjectileVisuals.prefab";

        public Guid ApplicableToTypeId => new Guid(Projectile.TypeIdString);
    }
}
