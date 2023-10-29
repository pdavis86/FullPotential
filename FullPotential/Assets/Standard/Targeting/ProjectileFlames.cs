using System;
using FullPotential.Api.Registry.Targeting;

namespace FullPotential.Standard.Targeting
{
    public class ProjectileFlames : ITargetingVisuals
    {
        public Guid TypeId => new Guid("5f901588-e3cc-442d-9330-7897f198b211");

        public string TypeName => nameof(ProjectileFlames);

        public string PrefabAddress => "Standard/Prefabs/Targeting/Projectile.prefab";

        public Guid TargetingTypeId => new Guid(Api.Gameplay.Targeting.Projectile.Id);
    }
}
