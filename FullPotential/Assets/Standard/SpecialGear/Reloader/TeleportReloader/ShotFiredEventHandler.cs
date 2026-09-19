using System;

using Cysharp.Threading.Tasks;

using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Combat.Events;
using FullPotential.Api.Gameplay.Events;

using Unity.Netcode;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Standard.SpecialGear.Reloader.TeleportReloader
{
    public class ShotFiredEventHandler : IEventHandler<ShotFiredEvent>
    {
        public NetworkLocation Location => NetworkLocation.Server;

        public Func<ShotFiredEvent, UniTask> BeforeHandlerAsync => null;

        public Func<ShotFiredEvent, UniTask> AfterHandlerAsync => HandleShotFiredAsync;

        private UniTask HandleShotFiredAsync(ShotFiredEvent eventArgs)
        {
            var reloader = eventArgs.Fighter.Inventory.GetItemInSlot<Api.Obsolete.Items.Types.SpecialGear>(SpecialSlots.RangedWeaponReloaderSlot.TypeIdString);

            if (reloader == null || reloader.RegistryTypeId != TeleportReloader.TypeIdString)
            {
                return UniTask.CompletedTask;
            }

            if (!eventArgs.Fighter.ConsumeResource(reloader, true, !NetworkManager.Singleton.IsServer))
            {
                return UniTask.CompletedTask;
            }

            var fighter = eventArgs.Fighter;

            var reloadEventArgs = new ReloadEvent(fighter, eventArgs.SlotId);

            FighterBase.UpdateAmmoCounts(reloadEventArgs);

            return UniTask.CompletedTask;
        }
    }
}
