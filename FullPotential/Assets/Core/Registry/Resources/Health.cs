using System;
using System.Drawing;
using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Registry.Gameplay;

namespace FullPotential.Core.Registry.Resources
{
    public class Health : IResourceType
    {
        private static readonly Guid Id = new Guid(ResourceTypeIds.HealthId);
        private static readonly Color ResourceColor = Color.FromArgb(156, 42, 50);

        public Guid TypeId => Id;

        public Color Color => ResourceColor;

        public bool IsCraftable => true;

        public string ItemInHandDefaultPrefab => null;

        public Action<LivingEntityBase> ReplenishBehaviour => null;
    }
}
