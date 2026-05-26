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
    public class ShotFiredEventHandler : IEventHandler
    {
        public NetworkLocation Location => NetworkLocation.Server;

        public Func<IEventHandlerArgs, UniTask> BeforeHandlerAsync => null;

        public Func<IEventHandlerArgs, UniTask> AfterHandlerAsync => HandleShotFiredAsync;

        private UniTask HandleShotFiredAsync(IEventHandlerArgs eventArgs)
        {
            var shotFiredEventArgs = (ShotFiredEventArgs)eventArgs;

            var reloader = shotFiredEventArgs.Fighter.Inventory.GetItemInSlot<Api.Items.Types.SpecialGear>(SpecialSlots.RangedWeaponReloaderSlot.TypeIdString);

            if (reloader == null || reloader.RegistryTypeId != TeleportReloader.TypeIdString)
            {
                return UniTask.CompletedTask;
            }

            if (!shotFiredEventArgs.Fighter.ConsumeResource(reloader, true, !NetworkManager.Singleton.IsServer))
            {
                return UniTask.CompletedTask;
            }

            var fighter = shotFiredEventArgs.Fighter;

            var reloadEventArgs = new ReloadEventArgs(fighter, shotFiredEventArgs.SlotId);

            FighterBase.UpdateAmmoCounts(reloadEventArgs);

            return UniTask.CompletedTask;
        }
    }
}
