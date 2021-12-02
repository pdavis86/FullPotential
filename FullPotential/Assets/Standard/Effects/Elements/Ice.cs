using FullPotential.Api.Registry;
using System;

namespace FullPotential.Standard.Effects.Elements
{
    public class Ice : IElement
    {
        public Guid TypeId => new Guid("8acf4936-49f3-4eab-acbb-6c8aef97f1aa");

        public string TypeName => nameof(Ice);

        public bool IsSideEffect => false;

        public string LingeringTypeName => "Freeze";
    }
}
