using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Obsolete.Items.Types;

using UnityEngine;

namespace FullPotential.Api.Registry.Targeting
{
    public interface ITargetingBehaviour
    {
        public FighterBase SourceFighter { get; set; }

        public Consumer Consumer { get; set; }

        public Vector3 Direction { get; set; }
    }
}
