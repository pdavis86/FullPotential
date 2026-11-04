using System.Collections.Generic;

using FullPotential.Api.Registry.Effects;

namespace FullPotential.Api.Registry
{
    public interface IHasEffects
    {
        List<IEffectType> EffectTypes { get; set; }

        float GetEffectDuration();

        int GetSingleEffectValueChange();

        int GetPeriodicEffectValueChange();
    }
}
