using System;
using System.Drawing;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Registry.Resources;

namespace FullPotential.Core.Registry.Resources
{
    public class Energy : IResource
    {
        public Guid TypeId => ResourceTypeIds.Energy;

        public Color Color => Color.FromArgb(25, 118, 64);

        public string ItemInHandDefaultPrefab => "Core/Prefabs/Combat/GadgetInHand.prefab";
    }
}
