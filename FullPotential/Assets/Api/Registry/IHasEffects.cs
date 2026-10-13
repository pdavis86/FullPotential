using System.Collections.Generic;

using FullPotential.Api.Registry.Effects;

namespace FullPotential.Api.Registry
{
    public interface IHasEffects
    {
        //List<string> EffectIds { get; set; }

        List<IEffectType> EffectTypes { get; set; }
    }
}
