using System;

using Cysharp.Threading.Tasks;

using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Combat.Events;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Items.Types;

using Unity.Netcode;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Standard.SpecialGear.Reloader.TeleportReloader
{
    [SubscribeToEvent(FighterBase.ShotFiredEventId)]
    public class ShotFiredEventHandler : IEventHandler<ShotFiredEventArgs>
    {
        public NetworkLocation Location => NetworkLocation.Server;

        public Func<ShotFiredEventArgs, UniTask> BeforeHandlerAsync => null;

        public Func<ShotFiredEventArgs, UniTask> AfterHandlerAsync => HandleShotFiredAsync;

        private UniTask HandleShotFiredAsync(ShotFiredEventArgs eventArgs)
        {
            var reloader = eventArgs.Fighter.Inventory.GetItemInSlot<Api.Items.Types.SpecialGear>(SpecialSlots.RangedWeaponReloaderSlot.TypeIdString);

            if (reloader == null || reloader.RegistryTypeId != TeleportReloader.TypeIdString)
            {
                return UniTask.CompletedTask;
            }

            if (!eventArgs.Fighter.ConsumeResource(reloader, true, !NetworkManager.Singleton.IsServer))
            {
                return UniTask.CompletedTask;
            }

            var fighter = eventArgs.Fighter;

            var reloadEventArgs = new ReloadEventArgs(fighter, eventArgs.SlotId);

            FighterBase.UpdateAmmoCounts(reloadEventArgs);

            return UniTask.CompletedTask;
        }
    }
}
