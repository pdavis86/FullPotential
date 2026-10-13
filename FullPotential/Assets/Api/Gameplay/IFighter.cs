using FullPotential.Api.Gameplay.Behaviours;

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
