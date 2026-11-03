using FullPotential.Api.Registry.Shapes;
using FullPotential.Api.Registry.Targeting;
using FullPotential.Api.Registry.Visuals;

namespace FullPotential.Api.Items
{
    public interface IHasTargetingAndShape
    {
        const string PropertyKeyTargetingTypeId = "TargetingTypeId";
        const string PropertyKeyTargetingVisualsTypeId = "TargetingVisualsTypeId";
        const string PropertyKeyShapeTypeId = "ShapeTypeId";
        const string PropertyKeyShapeVisualsTypeId = "ShapeVisualsTypeId";


        ITargetingType TargetingType { get; set; }

        IItemVisualsType TargetingVisualsType { get; set; }

        IItemVisualsType ShapeVisualsType { get; set; }

        IShapeType ShapeType { get; set; }
    }
}
