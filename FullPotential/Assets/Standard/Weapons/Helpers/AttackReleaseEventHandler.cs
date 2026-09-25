using Cysharp.Threading.Tasks;

using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Input;
using FullPotential.Api.Items;

namespace FullPotential.Standard.Weapons.Helpers
{
    public class AttackReleaseEventHandler : IEventHandler<AttackReleaseInputEvent>
    {
        private readonly IEventBus _eventBus;
        private readonly IAttackHelper _attackHelper;

        public NetworkLocation Location => NetworkLocation.Both;

        public Timing Timing => Timing.Main;

        public AttackReleaseEventHandler(IEventBus eventBus, IAttackHelper attackHelper)
        {
            _eventBus = eventBus;
            _attackHelper = attackHelper;
        }

        public UniTask<HandlerResult> HandleEventAsync(AttackReleaseInputEvent eventArgs)
        {
            var item = eventArgs.Fighter.Inventory.GetItemInSlot(eventArgs.SlotId);

            if (item is IHasCharge itemWithCharge
                && itemWithCharge.IsChargePercentageUsed
                && itemWithCharge.ChargePercentage <= 0)
            {
                _eventBus.PublishAsync(new AttackHoldInputEvent(eventArgs.Fighter, eventArgs.SlotId));
                return UniTask.FromResult(new HandlerResult(NextAction.Cancel));
            }

            _attackHelper.AttackWithItemInHand(eventArgs.Fighter, eventArgs.SlotId);

            return UniTask.FromResult(new HandlerResult());
        }
    }
}
