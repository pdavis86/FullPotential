using System;
using FullPotential.Api.Registry.Targeting;

namespace FullPotential.Standard.Targeting
{
    public class BeamVisuals : ITargetingVisuals
    {
        public Guid TypeId => new Guid("e66ae393-7ed9-4329-a594-ad50a6f2a41b");

        public string TypeName => nameof(BeamVisuals);

        public string PrefabAddress => "Standard/Prefabs/SpellOrGadget/Beam.prefab";

        public Guid TargetingTypeId => new Guid(Api.Gameplay.Targeting.PointToPoint.Id);

        public bool IsParentedToSource => true;
    }
}
