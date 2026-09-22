using Cysharp.Threading.Tasks;

using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Gameplay.Player;
using FullPotential.Api.Input;
using FullPotential.Api.Items;
using FullPotential.Api.Logging;
using FullPotential.Api.Obsolete.Items.Types;

namespace FullPotential.Standard.Weapons.Helpers
{
    public class AttackHoldEventHandler : IEventHandler<AttackHoldEventArgs>
    {
        private readonly IAuditor _logger;
        private readonly IAttackHelper _attackHelper;

        public NetworkLocation Location => NetworkLocation.Both;

        public Timing Timing => Timing.Main;

        public AttackHoldEventHandler(IAuditorFactory auditorFactory, IAttackHelper attackHelper)
        {
            _logger = auditorFactory.Create(this);
            _attackHelper = attackHelper;
        }

        public UniTask<HandlerResult> HandleEventAsync(AttackHoldEventArgs eventArgs)
        {
            var item = eventArgs.Fighter.Inventory.GetItemInSlot(eventArgs.SlotId);
            var slotStatus = eventArgs.Fighter.GetSlotStatus(eventArgs.SlotId);

            if (item is Weapon weapon
                && weapon.Attributes.IsAutomatic)
            {
                slotStatus.StartIntraActionLoopAsync(new SlotIntraAction
                {
                    DelayAfter = weapon.GetDelayBetweenShots(),
                    ActionAsync = () =>
                    {
                        _attackHelper.AttackWithItemInHand(eventArgs.Fighter, eventArgs.SlotId, true);
                        return UniTask.CompletedTask;
                    },
                    AdditionalStopCondition = () => weapon.Ammo <= 0
                }).Forget();

                return UniTask.FromResult(new HandlerResult());
            }

            if (item is Consumer consumer)
            {
                if (!eventArgs.Fighter.ConsumeResource(consumer, isTest: true))
                {
                    return UniTask.FromResult(new HandlerResult());
                }
            }

            if (item is not IHasCharge itemWithCharge || !itemWithCharge.IsChargePercentageUsed)
            {
                _logger.Warn("Trying to attack hold an item that is not compatible");
                return UniTask.FromResult(new HandlerResult());
            }

            //Still cooling down
            if (itemWithCharge.ChargePercentage > 0)
            {
                return UniTask.FromResult(new HandlerResult());
            }

            slotStatus.StopCooldownLoop();
            slotStatus.StartChargeUpLoopAsync(itemWithCharge).Forget();

            return UniTask.FromResult(new HandlerResult());
        }
    }
}
