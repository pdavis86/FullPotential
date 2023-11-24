using System.Drawing;
using FullPotential.Api.Registry;

namespace FullPotential.Api.Gameplay.Combat
{
    public interface IResource : IRegisterable
    {
        public Color Color { get; }

        public string ItemInHandDefaultPrefab { get; }
    }
}
