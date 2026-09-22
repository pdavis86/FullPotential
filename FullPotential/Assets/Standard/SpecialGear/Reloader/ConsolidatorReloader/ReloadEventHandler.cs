using Cysharp.Threading.Tasks;

using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Input;
using FullPotential.Api.Obsolete.Items.Types;

using Unity.Netcode;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Standard.SpecialGear.Reloader.ConsolidatorReloader
{
    public class ReloadEventHandler : IEventHandler<ReloadEventArgs>
    {
        public NetworkLocation Location => NetworkLocation.Server;

        public Timing Timing => Timing.Before;

        public async UniTask<HandlerResult> HandleEventAsync(ReloadEventArgs eventArgs)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                return new HandlerResult();
            }

            var reloader = eventArgs.Fighter.Inventory.GetItemInSlot<Api.Obsolete.Items.Types.SpecialGear>(SpecialSlots.RangedWeaponReloaderSlot.TypeIdString);

            if (reloader == null || reloader.RegistryTypeId != ConsolidatorReloader.TypeIdString)
            {
                return new HandlerResult();
            }

            if (!eventArgs.Fighter.ConsumeResource(reloader))
            {
                return new HandlerResult();
            }

            var slotStatus = eventArgs.Fighter.GetSlotStatus(eventArgs.SlotId);

            slotStatus.IsBusy = true;

            var weapon = eventArgs.Fighter.Inventory.GetItemInSlot<Weapon>(eventArgs.SlotId);
            await UniTask.WaitForSeconds(weapon.GetReloadTime());

            ReloadEventArgs.UpdateAmmoCounts(eventArgs);

            slotStatus.IsBusy = false;

            return new HandlerResult(NextAction.Cancel);
        }
    }
}
