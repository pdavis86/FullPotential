using System;
using FullPotential.Api.Spells;

namespace FullPotential.Standard.Spells.Shapes
{
    public class Wall : ISpellShape
    {
        public Guid TypeId => new Guid("4aad2866-5903-4b79-bda2-e3dcab920d9e");

        public string TypeName => nameof(Wall);
    }
}
