using System;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Registry.Resources;

namespace FullPotential.Core.Registry.Resources
{
    public class Health : IResource
    {
        public Guid TypeId => ResourceTypeIds.Health;

        public string ItemInHandDefaultPrefab => null;
    }
}
