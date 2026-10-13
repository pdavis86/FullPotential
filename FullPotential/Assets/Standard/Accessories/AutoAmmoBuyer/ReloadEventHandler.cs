using System;
using System.Collections.Generic;

using Cysharp.Threading.Tasks;

using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Gameplay.Player;
using FullPotential.Api.Input;
using FullPotential.Api.Items;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Localization;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Weapons;
using FullPotential.Models.LivingEntity;
using FullPotential.Standard.Items;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Standard.Accessories.AutoAmmoBuyer
{
    public class ReloadEventHandler : IEventHandler<ReloadInputEvent>
    {
        private readonly ITypeRegistry _typeRegistry;
        private readonly ILocalizer _localizer;
        private readonly IItemFactory _itemFactory;

        public NetworkLocation Location => NetworkLocation.Server;

        public Timing Timing => Timing.Early;

        public ReloadEventHandler(ITypeRegistry typeRegistry, ILocalizer localizer, IItemFactory itemFactory)
        {
            _typeRegistry = typeRegistry;
            _localizer = localizer;
            _itemFactory = itemFactory;
        }

        public UniTask<HandlerResult> HandleEventAsync(ReloadInputEvent eventArgs)
        {
            var buyerSlotId = SlotStatus.GetSlotId(AutoAmmoBuyer.TypeIdString, 1);
            var buyerItem = eventArgs.Fighter.Inventory.GetItemInSlot(buyerSlotId);

            if (buyerItem == null)
            {
                return UniTask.FromResult(new HandlerResult());
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
                return UniTask.FromResult(new HandlerResult());
            }

            var newItemStack = _typeRegistry.GetItemTypeMapping<IAmmunitionType>().CreateNew();
            newItemStack.Id = Guid.NewGuid().ToString();
            newItemStack.RegistryType = ammoType;
            newItemStack.Name = _localizer.Translate(ammoType);
            newItemStack.Properties = new Dictionary<string, string> { { nameof(ItemStackBase.Count), ammoType.MaxStackSize.ToString() } };

            var addedToInventory = fighter.Inventory.ApplyInventoryChanges(new InventoryData
            {
                Items = new List<ItemData>() { _itemFactory.GetDataFromItem(equippedWeapon.CharacterId, newItemStack) }
            });

            if (addedToInventory)
            {
                //todo: zzz v0.9 - take money for ammo ItemStack
            }

            return UniTask.FromResult(new HandlerResult());
        }
    }
}
