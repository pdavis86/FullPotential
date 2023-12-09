using System;
using System.Drawing;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Registry.Resources;

namespace FullPotential.Core.Registry.Resources
{
    public class Health : IResource
    {
        public Guid TypeId => ResourceTypeIds.Health;

        public Color Color => Color.FromArgb(156, 42, 50);

        public bool IsCraftable => true;

        public string ItemInHandDefaultPrefab => null;

        public Action<LivingEntityBase> ReplenishBehaviour => _ => { };
    }
}
