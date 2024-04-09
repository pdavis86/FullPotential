using System;
using System.Drawing;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Registry.Gameplay;

namespace FullPotential.Standard.Resources
{
    public class Mana : IResource
    {
        public const string TypeIdString = "378443ee-7942-4cd5-977d-818ee03333e9";

        private static readonly Guid Id = new Guid(TypeIdString);
        private static readonly Color ResourceColor = Color.FromArgb(185, 36, 158);

        public Guid TypeId => Id;

        public Color Color => ResourceColor;

        public bool IsCraftable => true;

        public string ItemInHandDefaultPrefab => "Core/Prefabs/Combat/SpellInHand.prefab";

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
