using System;

// ReSharper disable UnusedMember.Global

namespace FullPotential.Api.Registry.Elements
{
    public interface IElement : IRegisterable
    {
        Type Opposite { get; }
    }
}
