using FullPotential.Api.Registry.Effects;

// ReSharper disable UnusedMember.Global

namespace FullPotential.Api.Registry.Elements
{
    public interface IElement : IEffect
    {
        string LingeringTypeName { get; }
    }
}
