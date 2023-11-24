using System;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Registry.Resources;

namespace FullPotential.Core.Registry.Resources
{
    public class Stamina : IResource
    {
        public Guid TypeId => ResourceTypeIds.Stamina;

        public string ItemInHandDefaultPrefab => null;
    }
}
