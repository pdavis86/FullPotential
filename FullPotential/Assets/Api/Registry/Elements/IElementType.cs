using System;

// ReSharper disable UnusedMember.Global

namespace FullPotential.Api.Registry.Elements
{
    public interface IElementType : IRegisterableType
    {
        Type Opposite { get; }
    }
}
