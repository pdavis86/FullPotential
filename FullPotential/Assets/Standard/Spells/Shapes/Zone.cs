using System;
using FullPotential.Api.Spells;

namespace FullPotential.Standard.Spells.Shapes
{
    public class Zone : ISpellShape
    {
        public Guid TypeId => new Guid("142aeb3b-84b1-43c6-ae91-388b0901fa52");

        public string TypeName => nameof(Zone);
    }
}
