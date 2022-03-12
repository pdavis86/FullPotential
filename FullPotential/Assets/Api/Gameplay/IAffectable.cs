using FullPotential.Api.Registry.Effects;

namespace FullPotential.Api.Gameplay
{
    public interface IAffectable
    {
        void ApplyEffect(IEffect effect);
    }
}
