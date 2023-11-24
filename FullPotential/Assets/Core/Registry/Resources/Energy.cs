using System;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Registry.Resources;

namespace FullPotential.Core.Registry.Resources
{
    public class Energy : IResource
    {
        public Guid TypeId => ResourceTypeIds.Energy;

        public string ItemInHandDefaultPrefab => "Core/Prefabs/Combat/GadgetInHand.prefab";
    }
}
