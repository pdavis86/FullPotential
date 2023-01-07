using System;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Api.Gameplay.Effects
{
    public class ActiveEffect
    {
        public Guid Id { get; set; }

        public IEffect Effect { get; set; }

        public DateTime Expiry { get; set; }

        public int Change { get; set; }

        public bool ShowExpiry { get; set; }
    }
}
