using System.Diagnostics;
using System.Threading;

using Cysharp.Threading.Tasks;

using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Combat.Events;
using FullPotential.Api.Items;
using FullPotential.Api.Logging;
using FullPotential.Api.Obsolete.Items.Types;

namespace FullPotential.Api.Gameplay.Player
{
    public class SlotStatus
    {
        private const float ChargeGaugeUpdateSeconds = 0.05f;

        private readonly IAuditor _logger;

        private CancellationTokenSource _preActionCts;
        private CancellationTokenSource _intraActionCts;
        private CancellationTokenSource _postActionCts;

        public FighterBase Fighter { get; private set; }

        public string SlotId { get; private set; }

        public bool IsBusy { get; set; }

        public bool IsConsumingResource { get; set; }

        public bool IsAutoFiring { get; set; }

        public SlotStatus(IAuditor logger, FighterBase fighter, string slotId)
        {
            _logger = logger;

            Fighter = fighter;
            SlotId = slotId;
        }

        public async UniTask StartChargeUpLoopAsync(IHasCharge item)
        {
            _logger.Debug("StartChargeUpLoopAsync");

            _preActionCts?.Cancel();
            _preActionCts = new CancellationTokenSource();

            var secondsToTake = item.GetChargeUpTime();
            var secondsUntilDone = secondsToTake * (100 - item.ChargePercentage) / 100f;
            var elapsedSeconds = secondsToTake - secondsUntilDone;

            Stopwatch sw = null;
            if (_logger.IsEnabled(AuditLevel.Debug))
            {
                sw = Stopwatch.StartNew();
            }

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

            if (_logger.IsEnabled(AuditLevel.Debug))
            {
                _logger.Debug($"Charged in: {sw.ElapsedMilliseconds}ms and should have taken {secondsUntilDone}s");
            }
        }

        public void StopChargeUpLoop()
        {
            _logger.Debug("StopChargeUpLoop");

            _preActionCts?.Cancel();
        }

        public async UniTask StartAutomaticWeaponFireAsync(Weapon weapon)
        {
            _logger.Debug("StartAutomaticWeaponFireAsync");

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
            _logger.Debug("StopAutomaticWeaponFire");

            _intraActionCts?.Cancel();
            IsAutoFiring = false;
        }

        public async UniTask StartCooldownLoopAsync(IHasCharge item)
        {
            _logger.Debug("StartCooldownLoopAsync");

            _postActionCts?.Cancel();
            _postActionCts = new CancellationTokenSource();

            var secondsToTake = item.GetCooldownTime();
            var secondsUntilDone = secondsToTake * item.ChargePercentage / 100f;
            var elapsedSeconds = secondsToTake - secondsUntilDone;

            Stopwatch sw = null;
            if (_logger.IsEnabled(AuditLevel.Debug))
            {
                sw = Stopwatch.StartNew();
            }

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

            if (_logger.IsEnabled(AuditLevel.Debug))
            {
                _logger.Debug($"Cooled in: {sw.ElapsedMilliseconds}ms and should have taken {secondsUntilDone}s");
            }
        }

        public void StopCooldownLoop()
        {
            _logger.Debug("StopCooldownLoop");

            _postActionCts?.Cancel();
        }

        public bool StopActiveConsumerBehaviour()
        {
            if (!IsConsumingResource)
            {
                return false;
            }

            _logger.Debug("StopActiveConsumerBehaviour");

            var activeConsumer = Fighter.Inventory.GetItemInSlot<Consumer>(SlotId);

            activeConsumer.StopStoppables();

            IsConsumingResource = false;

            return true;
        }

        public static async UniTask DefaultHandlerForReloadEventAsync(ReloadEvent reloadEvent)
        {
            var slotStatus = reloadEvent.Fighter.GetSlotStatus(reloadEvent.SlotId);
            slotStatus.IsBusy = true;

            var weapon = reloadEvent.Fighter.Inventory.GetItemInSlot<Weapon>(reloadEvent.SlotId);

            //Lose any remaining ammo
            weapon.UpdateAmmo(0);

            await UniTask.WaitForSeconds(weapon.GetReloadTime());

            FighterBase.UpdateAmmoCounts(reloadEvent);

            slotStatus.IsBusy = false;
        }
    }
}