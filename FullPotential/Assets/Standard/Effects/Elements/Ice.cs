using System;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Registry.Elements;

namespace FullPotential.Standard.Effects.Elements
{
    public class Ice : IElement
    {
        public Guid TypeId => new Guid("8acf4936-49f3-4eab-acbb-6c8aef97f1aa");

        public string TypeName => nameof(Ice);

        public Affect Affect => Affect.Elemental;

        public string LingeringTypeName => "Freeze";

        public Type Opposite => typeof(Fire);
    }
}
