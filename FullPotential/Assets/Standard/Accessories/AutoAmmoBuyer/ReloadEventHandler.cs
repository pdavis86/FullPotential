using System;
using FullPotential.Api.Gameplay.Combat.EventArgs;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Ioc;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Localization;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Weapons;
using FullPotential.Api.Utilities.Extensions;

namespace FullPotential.Standard.Accessories.AutoAmmoBuyer
{
    public class ReloadEventHandler : IEventHandler
    {
        public Action<IEventHandlerArgs> BeforeHandler => HandleReloadBefore;

        public Action<IEventHandlerArgs> AfterHandler => null;

        private void HandleReloadBefore(IEventHandlerArgs eventArgs)
        {
            var reloadEventArgs = (ReloadEventArgs)eventArgs;

            var slotId = Accessory.GetSlotId(AutoAmmoBuyer.TypeIdString, 1);
            var buyerItem = reloadEventArgs.Fighter.Inventory.GetItemInSlot(slotId);

            if (buyerItem == null)
            {
                return;
            }

            var typeRegistry = DependenciesContext.Dependencies.GetService<ITypeRegistry>();

            var handStatus = reloadEventArgs.Fighter.GetHandStatus(reloadEventArgs.IsLeftHand);

            var ammoTypeId = handStatus.EquippedWeapon.GetAmmoTypeId();
            var ammoType = typeRegistry.GetRegisteredByTypeId<IAmmunition>(ammoTypeId);

            var ammoRemaining = reloadEventArgs.Fighter.Inventory.GetItemStackTotal(ammoTypeId);

            if (ammoRemaining < ammoType.MaxStackSize)
            {
                var localizer = DependenciesContext.Dependencies.GetService<ILocalizer>();

                reloadEventArgs.Fighter.Inventory.GiveItemStack(new ItemStack
                {
                    RegistryType = ammoType,
                    Id = Guid.NewGuid().ToMinimisedString(),
                    BaseName = localizer.Translate(ammoType),
                    Count = ammoType.MaxStackSize
                });
            }

            //todo: zzz v0.5 take money for ammo ItemStack
        }
    }
}
