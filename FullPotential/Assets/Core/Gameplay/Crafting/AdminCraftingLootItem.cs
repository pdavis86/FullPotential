using System.Collections.Generic;

using FullPotential.Api.Gameplay;
using FullPotential.Api.Items;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Registry.Gameplay;
using FullPotential.Api.Registry.Shapes;
using FullPotential.Api.Registry.Targeting;
using FullPotential.Api.Registry.Visuals;

namespace FullPotential.Core.Gameplay.Crafting
{
    // todo: move to standard
    public class AdminCraftingLootItem : ItemBase, ICombatItem // todo: CombatItemBase, IHasTargetingAndShape, IValuePoolConsumer
    {
        public ITargetingType TargetingType { get; set; }
        public IItemVisualsType TargetingVisualsType { get; set; }
        public IItemVisualsType ShapeVisualsType { get; set; }
        public IShapeType ShapeType { get; set; }
        public IValuePoolType ValuePoolType { get; set; }
        public bool IsTwoHanded { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public string ShapeCode { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public List<IEffectType> EffectTypes { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public bool IsChargePercentageUsed => throw new System.NotImplementedException();

        public int ChargePercentage { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public float GetAmmoPerSecond()
        {
            throw new System.NotImplementedException();
        }

        public float GetChargeUpTime()
        {
            throw new System.NotImplementedException();
        }

        public float GetCooldownTime()
        {
            throw new System.NotImplementedException();
        }

        public float GetDamagePerSecond()
        {
            throw new System.NotImplementedException();
        }

        public float GetEffectDuration()
        {
            throw new System.NotImplementedException();
        }

        public int GetNameHash()
        {
            throw new System.NotImplementedException();
        }

        public int GetPeriodicEffectValueChange()
        {
            throw new System.NotImplementedException();
        }

        public int GetSingleEffectValueChange()
        {
            throw new System.NotImplementedException();
        }

        public override string GetTextForUiComponent(string id, IPlayerFighter playerFighter)
        {
            throw new System.NotImplementedException();
        }
    }
}
