using System;
using System.Drawing;

using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Registry.Gameplay;

namespace FullPotential.Core.Registry.ValuePools
{
    public class Health : IValuePoolType
    {
        private static readonly Guid Id = new Guid(ValuePoolTypeIds.HealthId);
        private static readonly Color ValuePoolBackgroundColor = Color.FromArgb(156, 42, 50); // Red
        private static readonly Color ValuePoolTextColor = Color.FromArgb(255, 255, 255); // White

        public Guid TypeId => Id;

        public Color BackgroundColor => ValuePoolBackgroundColor;

        public Color TextColor => ValuePoolTextColor;

        public bool IsCraftable => true;

        public string ItemInHandDefaultPrefab => null;

        public Action<LivingEntityBase> ReplenishBehaviour => null;
    }
}
