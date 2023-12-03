using System;
using FullPotential.Api.Data;
using FullPotential.Api.Gameplay.Combat.EventArgs;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Localization;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Weapons;
using FullPotential.Api.Ui;
using FullPotential.Api.Utilities.Extensions;
using Unity.Netcode;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Standard.Accessories.AutoAmmoBuyer
{
    public class ReloadEventHandler : IEventHandler
    {
        private readonly ITypeRegistry _typeRegistry;
        private readonly ILocalizer _localizer;

        public NetworkLocation Location => NetworkLocation.Server;

        public Action<IEventHandlerArgs> BeforeHandler => HandleReloadBefore;

        public Action<IEventHandlerArgs> AfterHandler => null;

        public ReloadEventHandler(ITypeRegistry typeRegistry, ILocalizer localizer)
        {
            _typeRegistry = typeRegistry;
            _localizer = localizer;
        }

        private void HandleReloadBefore(IEventHandlerArgs eventArgs)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                return;
            }

            var reloadEventArgs = (ReloadEventArgs)eventArgs;

            var buyerSlotId = Accessory.GetSlotId(AutoAmmoBuyer.TypeIdString, 1);
            var buyerItem = reloadEventArgs.Fighter.Inventory.GetItemInSlot(buyerSlotId);

            if (buyerItem == null)
            {
                return;
            }

            var fighter = reloadEventArgs.Fighter;

            var slotId = reloadEventArgs.IsLeftHand ? HandSlotIds.LeftHand : HandSlotIds.RightHand;
            var equippedWeapon = (Weapon)fighter.Inventory.GetItemInSlot(slotId);

            var ammoTypeId = equippedWeapon.GetAmmoTypeId();
            var ammoType = _typeRegistry.GetRegisteredByTypeId<IAmmunition>(ammoTypeId);

            var ammoRemaining = fighter.Inventory.GetItemStackTotal(ammoTypeId);

            if (ammoRemaining >= equippedWeapon.GetAmmoMax())
            {
                return;
            }

            var newItemStack = new ItemStack
            {
                RegistryType = ammoType,
                Id = Guid.NewGuid().ToMinimisedString(),
                BaseName = _localizer.Translate(ammoType),
                Count = ammoType.MaxStackSize
            };

            fighter.Inventory.ApplyInventoryChanges(new InventoryChanges
            {
                ItemStacks = new[] { newItemStack }
            });

            //todo: zzz v0.5 take money for ammo ItemStack
        }
    }
}
