using System;
using System.Drawing;

using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Registry.Gameplay;

namespace FullPotential.Standard.ValuePools
{
    public class Mana : IValuePoolType
    {
        public static readonly ValuePoolTypeId Id = ValuePoolTypeId.FromString(TypeIdString);

        private const string TypeIdString = "378443ee-7942-4cd5-977d-818ee03333e9";
        private static readonly Color ValuePoolBackgroundColor = Color.FromArgb(39, 80, 147); // Blue
        private static readonly Color ValuePoolTextColor = Color.FromArgb(255, 255, 255); // White

        public Guid TypeId { get; } = new Guid(TypeIdString);

        public Color BackgroundColor => ValuePoolBackgroundColor;

        public Color TextColor => ValuePoolTextColor;

        public bool IsCraftable => true;

        public string ItemInHandDefaultPrefab => "Core/Prefabs/Combat/SpellInHand.prefab";

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
