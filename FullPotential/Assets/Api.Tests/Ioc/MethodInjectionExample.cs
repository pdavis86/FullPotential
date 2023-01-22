using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Gameplay.Inventory;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Api.Tests.Ioc
{
    public class MethodInjectionExample
    {
        // ReSharper disable UnassignedField.Global
        public IPlayerFighter Fighter;
        public IPlayerInventory Inventory;
        // ReSharper restore UnassignedField.Global

        //public void InjectDependencies(IPlayerFighter fighter, IPlayerInventory inventory)
        //{
        //    Fighter = fighter;
        //    Inventory = inventory;
        //}
    }
}
