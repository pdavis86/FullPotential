using System;

using FullPotential.Api.Registry.Elements;

namespace FullPotential.Standard.Effects.Elements
{
    public class Ice : IElementType
    {
        public Guid TypeId { get; } = new Guid("8acf4936-49f3-4eab-acbb-6c8aef97f1aa");

        public Type Opposite => typeof(Fire);
    }
}
