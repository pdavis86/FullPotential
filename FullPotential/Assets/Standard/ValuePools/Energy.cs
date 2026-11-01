using System;
using System.Drawing;

using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Registry.Gameplay;

namespace FullPotential.Standard.ValuePools
{
    public class Energy : IValuePoolType
    {
        public const string TypeIdString = "89ec3ecf-badb-4e55-91b0-b288ca358010";

        private static readonly Guid Id = new Guid(TypeIdString);
        private static readonly Color ValuePoolBackgroundColor = Color.FromArgb(255, 255, 0); // Yellow
        private static readonly Color ValuePoolTextColor = Color.FromArgb(0, 0, 0); // Black

        public Guid TypeId => Id;

        public Color BackgroundColor => ValuePoolBackgroundColor;

        public Color TextColor => ValuePoolTextColor;

        public bool IsCraftable => true;

        public string ItemInHandDefaultPrefab => "Core/Prefabs/Combat/GadgetInHand.prefab";

        public Action<LivingEntityBase> ReplenishBehaviour => PerformReplenish;

        private void PerformReplenish(LivingEntityBase livingEntity)
        {
            if (!livingEntity.IsConsumingValuePool(TypeIdString)
                && livingEntity.GetValuePoolValue(TypeIdString) < livingEntity.GetValuePoolMax(TypeIdString))
            {
                //todo: zzz v0.8 - trait-based valuePool recharge
                livingEntity.TriggerValuePoolValueUpdate(TypeIdString, 1, livingEntity.name, null);
            }
        }
    }
}
