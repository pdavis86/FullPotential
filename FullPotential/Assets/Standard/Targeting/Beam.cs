using System;
using FullPotential.Api.Gameplay.Targeting;
using FullPotential.Api.Registry.Targeting;

namespace FullPotential.Standard.Targeting
{
    public class Beam : ITargetingVisuals<PointToPoint>
    {
        public Guid TypeId => new Guid("e66ae393-7ed9-4329-a594-ad50a6f2a41b");

        public string TypeName => nameof(Beam);

        public bool IsParentedToSource => true;

        public string PrefabAddress => "Standard/Prefabs/SpellOrGadget/Beam.prefab";
    }
}
