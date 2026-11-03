using System;

using FullPotential.Api.Registry;
using FullPotential.Standard.Targeting;

namespace FullPotential.Standard.TargetingVisuals
{
    public class ProjectileFlames : IItemVisualsType
    {
        public Guid TypeId { get; } = new Guid("5f901588-e3cc-442d-9330-7897f198b211");

        public string PrefabAddress => "Standard/Prefabs/Targeting/ProjectileVisuals.prefab";

        public string ApplicableToTypeIdString => Projectile.TypeIdString;
    }
}
