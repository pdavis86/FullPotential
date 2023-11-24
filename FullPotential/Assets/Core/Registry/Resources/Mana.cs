using System;
using System.Drawing;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Registry.Resources;

namespace FullPotential.Core.Registry.Resources
{
    public class Mana : IResource
    {
        public Guid TypeId => ResourceTypeIds.Mana;

        public Color Color => Color.FromArgb(185, 36, 158);

        public string ItemInHandDefaultPrefab => "Core/Prefabs/Combat/SpellInHand.prefab";
    }
}
