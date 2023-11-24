using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Items.Types;
using UnityEngine;

// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable UnusedMember.Global

namespace FullPotential.Api.Registry.Shapes
{
    public interface IShapeBehaviour
    {
        public FighterBase SourceFighter { get; set; }

        public Consumer Consumer { get; set; }

        public Vector3 Direction { get; set; }
    }
}
