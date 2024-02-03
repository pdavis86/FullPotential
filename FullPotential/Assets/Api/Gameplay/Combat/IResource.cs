using System;
using System.Drawing;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Registry;

namespace FullPotential.Api.Gameplay.Combat
{
    public interface IResource : IRegisterable
    {
        Color Color { get; }

        bool IsCraftable { get; }

        string ItemInHandDefaultPrefab { get; }

        Action<LivingEntityBase> ReplenishBehaviour { get; }
    }
}
