using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Items.Types;
using UnityEngine;

namespace FullPotential.Api.Gameplay.Shapes
{
    public interface IShapeBehaviour
    {
        public IFighter SourceFighter { get; set; }

        public Consumer Consumer { get; set; }

        public Vector3 Direction { get; set; }
    }
}
