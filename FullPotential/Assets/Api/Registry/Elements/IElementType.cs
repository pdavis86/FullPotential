using System;


namespace FullPotential.Api.Registry.Elements
{
    public interface IElementType : IRegisterableType
    {
        Type Opposite { get; }
    }
}
