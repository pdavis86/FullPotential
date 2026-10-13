using System.Collections.Generic;

using FullPotential.Api.Gameplay;
using FullPotential.Api.Items;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Registry.Shapes;
using FullPotential.Api.Registry.Targeting;

namespace FullPotential.Standard.Items
{
    public class Loot : ItemBase, IHasTargetingAndShape, IHasEffects
    {
        //public string TargetingTypeId { get; set; }
        public ITargetingType TargetingType { get; set; }
        //public string TargetingVisualsTypeId { get; set; }
        public IItemVisuals TargetingVisualsType { get; set; }
        //public string ShapeVisualsTypeId { get; set; }
        public IItemVisuals ShapeVisualsType { get; set; }
        //public string ShapeTypeId { get; set; }
        public IShapeType ShapeType { get; set; }
        //public List<string> EffectIds { get; set; }
        public List<IEffectType> EffectTypes { get; set; }

        public override string GetTextForUiComponent(string id, IPlayerFighter playerFighter)
        {
            return null;
        }
    }
}
