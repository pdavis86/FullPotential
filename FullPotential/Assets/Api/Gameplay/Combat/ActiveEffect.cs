using System;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Api.Gameplay.Combat
{
    public class ActiveEffect
    {
        public Guid Id { get; set; }

        public IEffect Effect { get; set; }

        public DateTime Expiry { get; set; }

        public int Change { get; set; }
    }
}
