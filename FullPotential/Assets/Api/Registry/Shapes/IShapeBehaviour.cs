using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Items;

using UnityEngine;

namespace FullPotential.Api.Registry.Shapes
{
    public interface IShapeBehaviour
    {
        FighterBase SourceFighter { get; set; }

        IConsumer Consumer { get; set; }

        Vector3 Direction { get; set; }
    }
}
