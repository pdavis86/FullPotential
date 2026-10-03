using Cysharp.Threading.Tasks;

using FullPotential.Api.GameManagement;
using FullPotential.Api.Gameplay.Combat.Events;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Gameplay.Player.Events;
using FullPotential.Api.Utilities.Extensions;

namespace FullPotential.Core.Gameplay.Combat.Events
{
    public class EntityDiedEventDefaultHandler : IEventHandler<EntityDiedAfterEvent>
    {
        private readonly IGameManager _gameManager;
        private readonly IEventBus _eventbus;

        public NetworkLocation Location => NetworkLocation.Client;

        public Timing Timing => Timing.Main;

        public EntityDiedEventDefaultHandler(IGameManager gameManager, IEventBus eventBus)
        {
            _gameManager = gameManager;
            _eventbus = eventBus;
        }

        public UniTask<HandlerResult> HandleEventAsync(EntityDiedAfterEvent eventArgs)
        {
            var victimName = eventArgs.LivingEntity.name;

            if (victimName == _gameManager.GetLocalPlayerGameObject().name)
            {
                if (eventArgs.LastDamageSourceName == victimName)
                {
                    _eventbus.Publish(eventArgs.LastDamageItemName.IsNullOrWhiteSpace()
                        ? new ShowUiAlertEvent("ui.alert.attack.youkilledyourself")
                        : new ShowUiAlertEvent("ui.alert.attack.youkilledyourselfusing", new[] { eventArgs.LastDamageItemName }));
                    return UniTask.FromResult(new HandlerResult());
                }

                _eventbus.Publish(eventArgs.LastDamageItemName.IsNullOrWhiteSpace()
                    ? new ShowUiAlertEvent("ui.alert.attack.youwerekilledby", new[] { eventArgs.LastDamageSourceName })
                    : new ShowUiAlertEvent("ui.alert.attack.youwerekilledbyusing", new[] { eventArgs.LastDamageSourceName, eventArgs.LastDamageItemName }));
                return UniTask.FromResult(new HandlerResult());
            }

            if (eventArgs.LastDamageSourceName == victimName)
            {
                _eventbus.Publish(eventArgs.LastDamageItemName.IsNullOrWhiteSpace()
                    ? new ShowUiAlertEvent("ui.alert.attack.victimsuicide", new[] { victimName })
                    : new ShowUiAlertEvent("ui.alert.attack.victimsuicideusing", new[] { victimName, eventArgs.LastDamageItemName }));
                return UniTask.FromResult(new HandlerResult());
            }

            _eventbus.Publish(eventArgs.LastDamageItemName.IsNullOrWhiteSpace()
                ? new ShowUiAlertEvent("ui.alert.attack.victimkilledby", new[] { victimName, eventArgs.LastDamageSourceName })
                : new ShowUiAlertEvent("ui.alert.attack.victimkilledbyusing", new[] { victimName, eventArgs.LastDamageSourceName, eventArgs.LastDamageItemName }));
            return UniTask.FromResult(new HandlerResult());
        }
    }
}
