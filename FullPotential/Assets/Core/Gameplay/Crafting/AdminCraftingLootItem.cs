using FullPotential.Api.Gameplay;
using FullPotential.Api.Items;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Gameplay;
using FullPotential.Api.Registry.Shapes;
using FullPotential.Api.Registry.Targeting;

namespace FullPotential.Core.Gameplay.Crafting
{
    public class AdminCraftingLootItem : CombatItemBase, IHasTargetingAndShape, IResourceConsumer
    {
        public ITargetingType TargetingType { get; set; }
        public IItemVisualsType TargetingVisualsType { get; set; }
        public IItemVisualsType ShapeVisualsType { get; set; }
        public IShapeType ShapeType { get; set; }
        public IResourceType ResourceType { get; set; }

        public override string GetTextForUiComponent(string id, IPlayerFighter playerFighter)
        {
            throw new System.NotImplementedException();
        }
    }
}
