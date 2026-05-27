using System.Threading;

using Cysharp.Threading.Tasks;

using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Combat.Events;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Items;
using FullPotential.Api.Items.Types;

using UnityEngine;

namespace FullPotential.Api.Gameplay.Player
{
    public class SlotStatus
    {
        private const float ChargeGaugeUpdateSeconds = 0.05f;

        private CancellationTokenSource _preActionCts;
        private CancellationTokenSource _intraActionCts;
        private CancellationTokenSource _postActionCts;

        public FighterBase Fighter { get; private set; }

        public string SlotId { get; private set; }

        public bool IsBusy { get; set; }

        public bool IsConsumingResource { get; set; }

        public bool IsAutoFiring { get; set; }

        public SlotStatus(FighterBase fighter, string slotId)
        {
            Fighter = fighter;
            SlotId = slotId;
        }

        public async UniTask StartChargeUpLoopAsync(IHasCharge item)
        {
            // todo: remove debugging
            Debug.Log("StartChargeUpLoopAsync");

            _preActionCts?.Cancel();
            _preActionCts = new CancellationTokenSource();

            var secondsToTake = item.GetChargeUpTime();
            var secondsUntilDone = secondsToTake * (100 - item.ChargePercentage) / 100f;
            var elapsedSeconds = secondsToTake - secondsUntilDone;

            //var sw = System.Diagnostics.Stopwatch.StartNew();

            while (item.ChargePercentage < 100 && !_preActionCts.IsCancellationRequested)
            {
                await UniTask.WaitForSeconds(ChargeGaugeUpdateSeconds, cancellationToken: _preActionCts.Token);

                if (_preActionCts.IsCancellationRequested)
                {
                    return;
                }

                elapsedSeconds += ChargeGaugeUpdateSeconds;
                item.ChargePercentage = (int)(elapsedSeconds / secondsToTake * 100);
            }

            //Debug.Log($"Charged in: {sw.ElapsedMilliseconds}ms and should have taken {secondsUntilDone}s");
        }

        public void StopChargeUpLoop()
        {
            // todo: remove debugging
            Debug.Log("StopChargeUpLoop");

            _preActionCts?.Cancel();
        }

        public async UniTask StartAutomaticWeaponFireAsync(Weapon weapon)
        {
            // todo: remove debugging
            Debug.Log("StartAutomaticWeaponFireAsync");

            _intraActionCts?.Cancel();
            _intraActionCts = new CancellationTokenSource();

            var delay = weapon.GetDelayBetweenShots();

            IsAutoFiring = true;

            while (weapon.Ammo > 0 && !_intraActionCts.IsCancellationRequested)
            {
                Fighter.AttackWithItemInHand(SlotId);
                await UniTask.WaitForSeconds(delay, cancellationToken: _intraActionCts.Token);
            }
        }

        public void StopAutomaticWeaponFire()
        {
            // todo: remove debugging
            Debug.Log("StopAutomaticWeaponFire");

            _intraActionCts?.Cancel();
            IsAutoFiring = false;
        }

        public async UniTask StartCooldownLoopAsync(IHasCharge item)
        {
            // todo: remove debugging
            Debug.Log("StartCooldownLoopAsync");

            _postActionCts?.Cancel();
            _postActionCts = new CancellationTokenSource();

            var secondsToTake = item.GetCooldownTime();
            var secondsUntilDone = secondsToTake * item.ChargePercentage / 100f;
            var elapsedSeconds = secondsToTake - secondsUntilDone;

            //var sw = System.Diagnostics.Stopwatch.StartNew();

            while (item.ChargePercentage > 0 && !_postActionCts.IsCancellationRequested)
            {
                await UniTask.WaitForSeconds(ChargeGaugeUpdateSeconds, cancellationToken: _postActionCts.Token);

                if (_postActionCts.IsCancellationRequested)
                {
                    return;
                }

                elapsedSeconds += ChargeGaugeUpdateSeconds;
                item.ChargePercentage = 100 - (int)(elapsedSeconds / secondsToTake * 100);
            }

            //Debug.Log($"Cooled in: {sw.ElapsedMilliseconds}ms and should have taken {secondsUntilDone}s");
        }

        public void StopCooldownLoop()
        {
            // todo: remove debugging
            Debug.Log("StopCooldownLoop");

            _postActionCts?.Cancel();
        }

        public bool StopActiveConsumerBehaviour()
        {
            // todo: remove debugging
            Debug.Log("StopActiveConsumerBehaviour");

            if (!IsConsumingResource)
            {
                return false;
            }

            var activeConsumer = Fighter.Inventory.GetItemInSlot<Consumer>(SlotId);

            activeConsumer.StopStoppables();

            IsConsumingResource = false;

            return true;
        }

        public static async UniTask DefaultHandlerForReloadEventAsync(ReloadEventArgs eventArgs)
        {
            // todo: remove debugging
            Debug.Log("Reload");

            var slotStatus = eventArgs.Fighter.GetSlotStatus(eventArgs.SlotId);
            slotStatus.IsBusy = true;

            var weapon = eventArgs.Fighter.Inventory.GetItemInSlot<Weapon>(eventArgs.SlotId);

            //Lose any remaining ammo
            weapon.UpdateAmmo(0);

            await UniTask.WaitForSeconds(weapon.GetReloadTime());

            FighterBase.UpdateAmmoCounts(eventArgs);

            slotStatus.IsBusy = false;
        }
    }
}