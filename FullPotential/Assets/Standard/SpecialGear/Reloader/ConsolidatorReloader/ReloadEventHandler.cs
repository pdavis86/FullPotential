using System;

using Cysharp.Threading.Tasks;

using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Combat.Events;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Ui;

using Unity.Netcode;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Standard.SpecialGear.Reloader.ConsolidatorReloader
{
    public class ReloadEventHandler : IEventHandler
    {
        public NetworkLocation Location => NetworkLocation.Server;

        public Func<IEventHandlerArgs, UniTask> BeforeHandlerAsync => HandleReloadBeforeAsync;

        public Func<IEventHandlerArgs, UniTask> AfterHandlerAsync => null;

        private async UniTask HandleReloadBeforeAsync(IEventHandlerArgs eventArgs)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                return;
            }

            var reloadArgs = (ReloadEventArgs)eventArgs;

            var reloader = reloadArgs.Fighter.Inventory.GetItemInSlot<Api.Items.Types.SpecialGear>(SpecialSlots.RangedWeaponReloaderSlot.TypeIdString);

            if (reloader == null || reloader.RegistryTypeId != ConsolidatorReloader.TypeIdString)
            {
                return;
            }

            if (!reloadArgs.Fighter.ConsumeResource(reloader))
            {
                return;
            }

            eventArgs.IsDefaultHandlerCancelled = true;

            var slotStatus = reloadArgs.Fighter.GetSlotStatus(reloadArgs.SlotId);

            slotStatus.IsBusy = true;

            var weapon = reloadArgs.Fighter.Inventory.GetItemInSlot<Weapon>(reloadArgs.SlotId);
            await UniTask.WaitForSeconds(weapon.GetReloadTime());

            FighterBase.UpdateAmmoCounts(reloadArgs);

            slotStatus.IsBusy = false;
        }
    }
}
