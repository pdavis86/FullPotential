using System;
using FullPotential.Api.Registry.Consumers;
using FullPotential.Api.Registry.Shapes;

namespace FullPotential.Api.Gameplay.Shapes
{
    public class Wall : IShape
    {
        public Guid TypeId { get; }
        public string TypeName { get; }
    }
}
