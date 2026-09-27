using System;
using System.Drawing;

using FullPotential.Api.Gameplay.Behaviours;

namespace FullPotential.Api.Registry.Gameplay
{
    public interface IResourceType : IRegisterableType
    {
        Color BackgroundColor { get; }

        Color TextColor { get; }

        bool IsCraftable { get; }

        string ItemInHandDefaultPrefab { get; }

        Action<LivingEntityBase> ReplenishBehaviour { get; }
    }
}
