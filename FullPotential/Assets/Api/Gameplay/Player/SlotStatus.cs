using System.Diagnostics;
using System.Threading;

using Cysharp.Threading.Tasks;

using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Combat.Events;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Gameplay.Inventory.Events;
using FullPotential.Api.Items;
using FullPotential.Api.Logging;
using FullPotential.Api.Obsolete.Items.Types;

namespace FullPotential.Api.Gameplay.Player
{
    public class SlotStatus
    {
        private const float ChargeGaugeUpdateSeconds = 0.05f;

        private readonly IAuditor _logger;
        private readonly IEventBus _eventBus;

        private CancellationTokenSource _preActionCts;
        private CancellationTokenSource _intraActionCts;
        private CancellationTokenSource _postActionCts;

        public FighterBase Fighter { get; private set; }

        public string SlotId { get; private set; }

        public bool IsBusy { get; private set; }

        // todo: private set?
        public bool IsConsumingResource { get; set; }

        // todo: private set?
        public bool IsIntraActionLooping { get; set; }

        public SlotStatus(IAuditor logger, IEventBus eventBus, FighterBase fighter, string slotId)
        {
            _logger = logger;
            _eventBus = eventBus;

            Fighter = fighter;
            SlotId = slotId;
        }

        public async UniTask StartChargeUpLoopAsync(IHasCharge item)
        {
            _logger.Debug($"StartChargeUpLoopAsync for item '{item}'");

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

                _eventBus.PublishAsync(new ItemChargePercentageChangeEvent(SlotId)).Forget();
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

        public async UniTask StartIntraActionLoopAsync(SlotIntraAction intraAction)
        {
            _logger.Debug("StartIntraActionLoopAsync");

            _intraActionCts?.Cancel();
            _intraActionCts = new CancellationTokenSource();

            IsIntraActionLooping = true;

            while (!_intraActionCts.IsCancellationRequested)
            {
                if (intraAction.DelayBefore.HasValue)
                {
                    await UniTask.WaitForSeconds(intraAction.DelayBefore.Value, cancellationToken: _intraActionCts.Token);
                }

                await intraAction.ActionAsync();

                if (intraAction.AdditionalStopCondition != null && intraAction.AdditionalStopCondition())
                {
                    StopIntraActionLoop();
                    return;
                }

                if (intraAction.DelayAfter.HasValue)
                {
                    await UniTask.WaitForSeconds(intraAction.DelayAfter.Value, cancellationToken: _intraActionCts.Token);
                }
            }
        }

        public void StopIntraActionLoop()
        {
            _logger.Debug("StopIntraActionLoop");

            _intraActionCts?.Cancel();
            IsIntraActionLooping = false;
        }

        public async UniTask StartCooldownLoopAsync(IHasCharge item)
        {
            _logger.Debug($"StartCooldownLoopAsync for item '{item}'");

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

                _eventBus.PublishAsync(new ItemChargePercentageChangeEvent(SlotId)).Forget();
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

        public void SetBusyState(bool isBusy)
        {
            IsBusy = isBusy;
            _eventBus.PublishAsync(new SlotBusyChangeEvent(SlotId, isBusy)).Forget();
        }
    }
}
