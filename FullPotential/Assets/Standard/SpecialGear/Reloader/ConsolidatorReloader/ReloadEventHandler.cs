using System;

using Cysharp.Threading.Tasks;

using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Combat.Events;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Items.Types;

using Unity.Netcode;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Standard.SpecialGear.Reloader.ConsolidatorReloader
{
    [SubscribeToEvent(FighterBase.ReloadEventId)]
    public class ReloadEventHandler : IEventHandler<ReloadEventArgs>
    {
        public NetworkLocation Location => NetworkLocation.Server;

        public Func<ReloadEventArgs, UniTask> BeforeHandlerAsync => HandleReloadBeforeAsync;

        public Func<ReloadEventArgs, UniTask> AfterHandlerAsync => null;

        private async UniTask HandleReloadBeforeAsync(ReloadEventArgs eventArgs)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                return;
            }

            var reloader = eventArgs.Fighter.Inventory.GetItemInSlot<Api.Items.Types.SpecialGear>(SpecialSlots.RangedWeaponReloaderSlot.TypeIdString);

            if (reloader == null || reloader.RegistryTypeId != ConsolidatorReloader.TypeIdString)
            {
                return;
            }

            if (!eventArgs.Fighter.ConsumeResource(reloader))
            {
                return;
            }

            eventArgs.IsDefaultHandlerCancelled = true;

            var slotStatus = eventArgs.Fighter.GetSlotStatus(eventArgs.SlotId);

            slotStatus.IsBusy = true;

            var weapon = eventArgs.Fighter.Inventory.GetItemInSlot<Weapon>(eventArgs.SlotId);
            await UniTask.WaitForSeconds(weapon.GetReloadTime());

            FighterBase.UpdateAmmoCounts(eventArgs);

            slotStatus.IsBusy = false;
        }
    }
}
