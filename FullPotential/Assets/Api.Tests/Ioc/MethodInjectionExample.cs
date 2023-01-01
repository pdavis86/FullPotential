using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Gameplay.Inventory;

namespace FullPotential.Api.Tests.Ioc
{
    public class MethodInjectionExample
    {
        public IPlayerFighter Fighter;
        public IPlayerInventory Inventory;

        public void InjectDependencies(IPlayerFighter fighter, IPlayerInventory inventory)
        {
            Fighter = fighter;
            Inventory = inventory;
        }
    }
}
