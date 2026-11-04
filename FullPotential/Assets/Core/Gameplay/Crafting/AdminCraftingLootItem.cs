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
    public class AdminCraftingLootItem : ItemBase, ICombatItem
    {
        public ITargetingType TargetingType { get; set; }
        public IItemVisualsType TargetingVisualsType { get; set; }
        public IItemVisualsType ShapeVisualsType { get; set; }
        public IShapeType ShapeType { get; set; }
        public IValuePoolType ValuePoolType { get; set; }
        public bool IsTwoHanded { get; set; }
        public string ShapeCode { get; set; }
        public List<IEffectType> EffectTypes { get; set; }
        public bool IsChargePercentageUsed => throw new System.NotImplementedException();
        public int ChargePercentage { get; set; }

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
