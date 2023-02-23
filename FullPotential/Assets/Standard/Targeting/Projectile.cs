using System;
using FullPotential.Api.Registry.Targeting;

namespace FullPotential.Standard.Targeting
{
    public class Projectile : ITargetingVisuals<FullPotential.Api.Gameplay.Targeting.Projectile>
    {
        public Guid TypeId => new Guid("5f901588-e3cc-442d-9330-7897f198b211");

        public string TypeName => nameof(Projectile);

        public bool IsParentedToSource => false;

        public string PrefabAddress => "Standard/Prefabs/SpellOrGadget/Projectile.prefab";
    }
}
