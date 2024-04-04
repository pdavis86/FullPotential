using System;
using System.Drawing;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Combat;

namespace FullPotential.Standard.Resources
{
    public class Energy : IResource
    {
        private const string EnergyId = "89ec3ecf-badb-4e55-91b0-b288ca358010";

        public static readonly Guid Id = new Guid(EnergyId);

        public Guid TypeId => Id;

        public Color Color => Color.FromArgb(25, 118, 64);

        public bool IsCraftable => true;

        public string ItemInHandDefaultPrefab => "Core/Prefabs/Combat/GadgetInHand.prefab";

        public Action<LivingEntityBase> ReplenishBehaviour => PerformReplenish;

        private void PerformReplenish(LivingEntityBase livingEntity)
        {
            if (!livingEntity.IsConsumingResource(EnergyId)
                && livingEntity.GetResourceValue(EnergyId) < livingEntity.GetResourceMax(EnergyId))
            {
                //todo: zzz v0.4 - trait-based resource recharge
                livingEntity.AdjustResourceValue(EnergyId, 1);
            }
        }
    }
}
