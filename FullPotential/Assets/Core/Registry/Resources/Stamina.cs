using System;
using System.Drawing;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Registry.Resources;

namespace FullPotential.Core.Registry.Resources
{
    public class Stamina : IResource
    {
        public Guid TypeId => ResourceTypeIds.Stamina;

        public Color Color => Color.FromArgb(39, 80, 147);

        public string ItemInHandDefaultPrefab => null;
    }
}
