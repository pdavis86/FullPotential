using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Obsolete.Items.Types;

using UnityEngine;

namespace FullPotential.Api.Registry.Targeting
{
    public interface ITargetingBehaviour
    {
        FighterBase SourceFighter { get; set; }

        Consumer Consumer { get; set; }

        Vector3 Direction { get; set; }
    }
}
