using System.Collections.Generic;

using FullPotential.Api.Gameplay;
using FullPotential.Api.Items;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Registry.Shapes;
using FullPotential.Api.Registry.Targeting;

using FullPotential.Api.Registry.Visuals;

namespace FullPotential.Standard.Items
{
    public class Loot : ItemBase, IHasTargetingAndShape, IHasEffects
    {
        public ITargetingType TargetingType { get; set; }
        public IItemVisualsType TargetingVisualsType { get; set; }
        public IItemVisualsType ShapeVisualsType { get; set; }
        public IShapeType ShapeType { get; set; }
        public List<IEffectType> EffectTypes { get; set; }

        public float GetEffectDuration()
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
            return null;
        }
    }
}
