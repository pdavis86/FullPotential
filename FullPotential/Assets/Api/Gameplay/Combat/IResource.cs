using FullPotential.Api.Registry;

namespace FullPotential.Api.Gameplay.Combat
{
    public interface IResource : IRegisterable
    {
        public string ItemInHandDefaultPrefab { get; }
    }
}
