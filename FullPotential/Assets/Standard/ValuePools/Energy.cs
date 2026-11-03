using System;
using System.Drawing;

using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Registry.Gameplay;

namespace FullPotential.Standard.ValuePools
{
    public class Energy : IValuePoolType
    {
        public static readonly ValuePoolTypeId Id = ValuePoolTypeId.FromString(TypeIdString);

        private const string TypeIdString = "89ec3ecf-badb-4e55-91b0-b288ca358010";
        private static readonly Color ValuePoolBackgroundColor = Color.FromArgb(255, 255, 0); // Yellow
        private static readonly Color ValuePoolTextColor = Color.FromArgb(0, 0, 0); // Black

        public Guid TypeId { get; } = new Guid(TypeIdString);

        public Color BackgroundColor => ValuePoolBackgroundColor;

        public Color TextColor => ValuePoolTextColor;

        public bool IsCraftable => true;

        public string ItemInHandDefaultPrefab => "Core/Prefabs/Combat/GadgetInHand.prefab";

        public Action<LivingEntityBase> ReplenishBehaviour => PerformReplenish;

        private void PerformReplenish(LivingEntityBase livingEntity)
        {
            if (!livingEntity.IsConsumingValuePool(ValuePoolTypeId.FromString(TypeIdString))
                && livingEntity.GetValuePoolValue(ValuePoolTypeId.FromString(TypeIdString)) < livingEntity.GetValuePoolMax(ValuePoolTypeId.FromString(TypeIdString)))
            {
                //todo: zzz v0.8 - trait-based valuePool recharge
                livingEntity.TriggerValuePoolValueUpdate(ValuePoolTypeId.FromString(TypeIdString), 1, livingEntity.name, null);
            }
        }
    }
}
