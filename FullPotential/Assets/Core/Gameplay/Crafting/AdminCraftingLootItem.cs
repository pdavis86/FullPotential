using FullPotential.Api.Gameplay;
using FullPotential.Api.Items;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Registry.Gameplay;
using FullPotential.Api.Registry.Shapes;
using FullPotential.Api.Registry.Targeting;
using FullPotential.Api.Registry.Visuals;

namespace FullPotential.Core.Gameplay.Crafting
{
    public class AdminCraftingLootItem : CombatItemBase, IHasTargetingAndShape, IValuePoolConsumer
    {
        public ITargetingType TargetingType { get; set; }
        public IItemVisualsType TargetingVisualsType { get; set; }
        public IItemVisualsType ShapeVisualsType { get; set; }
        public IShapeType ShapeType { get; set; }
        public IValuePoolType ValuePoolType { get; set; }

        public override string GetTextForUiComponent(string id, IPlayerFighter playerFighter)
        {
            throw new System.NotImplementedException();
        }
    }
}
