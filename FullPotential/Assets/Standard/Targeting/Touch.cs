using System;
using FullPotential.Api.Gameplay.Targeting;
using FullPotential.Api.Registry.Targeting;

namespace FullPotential.Standard.Targeting
{
    public class Touch : ITargetingVisuals<Api.Gameplay.Targeting.Touch>
    {
        public Guid TypeId => new Guid("aeea26c2-8848-44a6-934e-df295cc72b02");

        public string TypeName => nameof(Self);

        public bool IsParentedToSource => false;

        public string PrefabAddress => "Standard/Prefabs/SpellOrGadget/Touch.prefab";
    }
}
