using System;
using System.Collections.Generic;

using Cysharp.Threading.Tasks;

using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Combat.Events;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Items;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Localization;
using FullPotential.Api.Obsolete.Items.Types;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Weapons;
using FullPotential.Models.Player;

using Unity.Netcode;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Standard.Accessories.AutoAmmoBuyer
{
    [SubscribeToEvent(FighterBase.ReloadEventId)]
    public class ReloadEventHandler : IEventHandler<ReloadEventArgs>
    {
        private readonly ITypeRegistry _typeRegistry;
        private readonly ILocalizer _localizer;
        private readonly IItemFactory _itemFactory;

        public NetworkLocation Location => NetworkLocation.Server;

        public Func<ReloadEventArgs, UniTask> BeforeHandlerAsync => HandleReloadBeforeAsync;

        public Func<ReloadEventArgs, UniTask> AfterHandlerAsync => null;

        public ReloadEventHandler(ITypeRegistry typeRegistry, ILocalizer localizer, IItemFactory itemFactory)
        {
            _typeRegistry = typeRegistry;
            _localizer = localizer;
            _itemFactory = itemFactory;
        }

        private UniTask HandleReloadBeforeAsync(ReloadEventArgs eventArgs)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                return UniTask.CompletedTask;
            }

            var buyerSlotId = Accessory.GetSlotId(AutoAmmoBuyer.TypeIdString, 1);
            var buyerItem = eventArgs.Fighter.Inventory.GetItemInSlot(buyerSlotId);

            if (buyerItem == null)
            {
                return UniTask.CompletedTask;
            }

            var fighter = eventArgs.Fighter;

            var equippedWeapon = fighter.Inventory.GetItemInSlot<Weapon>(eventArgs.SlotId);

            var ammoTypeId = equippedWeapon.WeaponType.AmmunitionTypeIdString;
            var ammoType = _typeRegistry.GetRegisteredByTypeId<IAmmunitionType>(ammoTypeId);

            var ammoRemaining = fighter.Inventory.GetItemStackTotal(ammoTypeId);

            //todo: zzz v0.9 - check if enough money for buying an ammo ItemStack
            var hasEnoughMoney = ammoRemaining > -1;

            if (ammoRemaining >= equippedWeapon.GetAmmoMax() || !hasEnoughMoney)
            {
                return UniTask.CompletedTask;
            }

            var newItemStack = new ItemStackBase
            {
                RegistryType = ammoType,
                Id = Guid.NewGuid().ToString(),
                BaseName = _localizer.Translate(ammoType),
                Count = ammoType.MaxStackSize
            };

            var addedToInventory = fighter.Inventory.ApplyInventoryChanges(new InventoryData
            {
                Items = new List<ItemData>() { _itemFactory.GetDataFromItem(equippedWeapon.CharacterId, newItemStack) }
            });

            if (addedToInventory)
            {
                //todo: zzz v0.9 - take money for ammo ItemStack
            }

            return UniTask.CompletedTask;
        }
    }
}
