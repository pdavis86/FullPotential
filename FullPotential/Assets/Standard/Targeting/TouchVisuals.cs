using System;
using FullPotential.Api.Registry.Targeting;

namespace FullPotential.Standard.Targeting
{
    public class TouchVisuals : ITargetingVisuals
    {
        public Guid TypeId => new Guid("aeea26c2-8848-44a6-934e-df295cc72b02");

        public string TypeName => nameof(TouchVisuals);

        public string PrefabAddress => "Standard/Prefabs/SpellOrGadget/Touch.prefab";

        public Guid TargetingGuid => new Guid(Api.Gameplay.Targeting.Touch.Id);

        public bool IsParentedToSource => false;
    }
}
