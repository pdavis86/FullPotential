using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Gameplay.Inventory;

namespace FullPotential.Api.Tests.Ioc
{
    public class ConstructorInjectionExample
    {
        public IPlayerFighter Fighter;
        public IPlayerInventory Inventory;

        public ConstructorInjectionExample(IPlayerFighter fighter, IPlayerInventory inventory)
        {
            Fighter = fighter;
            Inventory = inventory;
        }
    }
}
