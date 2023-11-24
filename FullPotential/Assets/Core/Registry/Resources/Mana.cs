using System;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Registry.Resources;

namespace FullPotential.Core.Registry.Resources
{
    public class Mana : IResource
    {
        public Guid TypeId => ResourceTypeIds.Mana;

        public string ItemInHandDefaultPrefab => "Core/Prefabs/Combat/SpellInHand.prefab";
    }
}
