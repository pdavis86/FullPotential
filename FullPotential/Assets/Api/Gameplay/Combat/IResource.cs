using System;
using System.Drawing;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Registry;

namespace FullPotential.Api.Gameplay.Combat
{
    public interface IResource : IRegisterable
    {
        public Color Color { get; }

        bool IsCraftable { get; }

        public string ItemInHandDefaultPrefab { get; }

        public Action<LivingEntityBase> ReplenishBehaviour { get; }
    }
}
