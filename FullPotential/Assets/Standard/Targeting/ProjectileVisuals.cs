using System;
using FullPotential.Api.Registry.Targeting;

namespace FullPotential.Standard.Targeting
{
    public class ProjectileVisuals : ITargetingVisuals
    {
        public Guid TypeId => new Guid("5f901588-e3cc-442d-9330-7897f198b211");

        public string TypeName => nameof(ProjectileVisuals);

        public string PrefabAddress => "Standard/Prefabs/SpellOrGadget/Projectile.prefab";

        public Guid TargetingTypeId => new Guid(Api.Gameplay.Targeting.Projectile.Id);

        public bool IsParentedToSource => false;
    }
}
