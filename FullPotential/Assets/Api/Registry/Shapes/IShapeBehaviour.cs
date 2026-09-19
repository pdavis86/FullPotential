using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Obsolete.Items.Types;

using UnityEngine;

// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable UnusedMember.Global

namespace FullPotential.Api.Registry.Shapes
{
    public interface IShapeBehaviour
    {
        FighterBase SourceFighter { get; set; }

        Consumer Consumer { get; set; }

        Vector3 Direction { get; set; }
    }
}
