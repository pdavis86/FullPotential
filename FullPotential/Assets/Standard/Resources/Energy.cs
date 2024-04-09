using System;
using System.Drawing;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Registry.Gameplay;

namespace FullPotential.Standard.Resources
{
    public class Energy : IResource
    {
        public const string TypeIdString = "89ec3ecf-badb-4e55-91b0-b288ca358010";

        private static readonly Guid Id = new Guid(TypeIdString);
        private static readonly Color ResourceColor = Color.FromArgb(25, 118, 64);

        public Guid TypeId => Id;

        public Color Color => ResourceColor;

        public bool IsCraftable => true;

        public string ItemInHandDefaultPrefab => "Core/Prefabs/Combat/GadgetInHand.prefab";

        public Action<LivingEntityBase> ReplenishBehaviour => PerformReplenish;

        private void PerformReplenish(LivingEntityBase livingEntity)
        {
            if (!livingEntity.IsConsumingResource(TypeIdString)
                && livingEntity.GetResourceValue(TypeIdString) < livingEntity.GetResourceMax(TypeIdString))
            {
                //todo: zzz v0.8 - trait-based resource recharge
                livingEntity.AdjustResourceValue(TypeIdString, 1);
            }
        }
    }
}
