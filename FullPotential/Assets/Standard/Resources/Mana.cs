using System;
using System.Drawing;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Combat;

namespace FullPotential.Standard.Resources
{
    public class Mana : IResource
    {
        private const string ManaId = "378443ee-7942-4cd5-977d-818ee03333e9";

        public static readonly Guid Id = new Guid(ManaId);

        public Guid TypeId => Id;

        public Color Color => Color.FromArgb(185, 36, 158);

        public bool IsCraftable => true;

        public string ItemInHandDefaultPrefab => "Core/Prefabs/Combat/SpellInHand.prefab";

        public Action<LivingEntityBase> ReplenishBehaviour => PerformReplenish;

        private void PerformReplenish(LivingEntityBase livingEntity)
        {
            if (!livingEntity.IsConsumingResource(ManaId)
                && livingEntity.GetResourceValue(ManaId) < livingEntity.GetResourceMax(ManaId))
            {
                //todo: zzz v0.5 - trait-based resource recharge
                livingEntity.AdjustResourceValue(ManaId, 1);
            }
        }
    }
}
