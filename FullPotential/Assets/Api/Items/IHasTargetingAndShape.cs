using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Shapes;
using FullPotential.Api.Registry.Targeting;

// ReSharper disable UnusedMemberInSuper.Global

namespace FullPotential.Api.Items
{
    public interface IHasTargetingAndShape
    {
        public const string PropertyKeyTargetingTypeId = "TargetingTypeId";
        public const string PropertyKeyTargetingVisualsTypeId = "TargetingVisualsTypeId";
        public const string PropertyKeyShapeTypeId = "ShapeTypeId";
        public const string PropertyKeyShapeVisualsTypeId = "ShapeVisualsTypeId";


        //string TargetingTypeId { get; set; }

        ITargetingType TargetingType { get; set; }

        //string TargetingVisualsTypeId { get; set; }

        IItemVisualsType TargetingVisualsType { get; set; }

        //string ShapeVisualsTypeId { get; set; }

        IItemVisualsType ShapeVisualsType { get; set; }

        //string ShapeTypeId { get; set; }

        IShapeType ShapeType { get; set; }
    }
}
