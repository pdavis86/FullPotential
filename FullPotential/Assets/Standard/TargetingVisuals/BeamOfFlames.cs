using System;
using FullPotential.Api.Registry.Targeting;
using FullPotential.Standard.Targeting;

namespace FullPotential.Standard.TargetingVisuals
{
    public class BeamOfFlames : ITargetingVisuals
    {
        public Guid TypeId => new Guid("4853434d-e4c9-47f9-a536-5d9a045cf3f2");

        public string PrefabAddress => "Standard/Prefabs/Targeting/BeamVisuals.prefab";

        public Guid ApplicableToTypeId => new Guid(PointToPoint.TypeIdString);
    }
}
