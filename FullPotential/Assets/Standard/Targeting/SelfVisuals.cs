using System;
using FullPotential.Api.Registry.Targeting;

namespace FullPotential.Standard.Targeting
{
    public class SelfVisuals : ITargetingVisuals
    {
        public Guid TypeId => new Guid("8fd83680-e4a2-4ce6-b2f9-5ae3b776507c");

        public string TypeName => nameof(SelfVisuals);

        public string PrefabAddress => "Standard/Prefabs/SpellOrGadget/Self.prefab";

        public Guid TargetingTypeId => new Guid(Api.Gameplay.Targeting.Self.Id);

        public bool IsParentedToSource => false;
    }
}
