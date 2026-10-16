using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Player;

using UnityEngine;

namespace FullPotential.Api.Gameplay
{
    public interface IFighter
    {
        InventoryBase Inventory { get; }

        Transform GraphicsTransform { get; }

        PositionTransforms Positions { get; }

        BodyPartTransforms BodyParts { get; }
    }
}
