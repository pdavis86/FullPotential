using System;
using FullPotential.Api.Registry.Targeting;
using FullPotential.Standard.Targeting;

namespace FullPotential.Standard.TargetingVisuals
{
    public class BeamOfFlames : ITargetingVisuals
    {
        private static readonly Guid Id = new Guid("4853434d-e4c9-47f9-a536-5d9a045cf3f2");

        public Guid TypeId => Id;

        public string PrefabAddress => "Standard/Prefabs/Targeting/BeamVisuals.prefab";

        public string ApplicableToTypeIdString => PointToPoint.TypeIdString;
    }
}
