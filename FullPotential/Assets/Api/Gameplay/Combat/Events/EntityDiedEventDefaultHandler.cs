using Cysharp.Threading.Tasks;

using FullPotential.Api.GameManagement;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Localization;
using FullPotential.Api.Utilities.Extensions;

using Unity.Netcode;

namespace FullPotential.Api.Gameplay.Combat.Events
{
    // todo: move "Handler : IEventHandler<" classes into Core
    public class EntityDiedEventDefaultHandler : IEventHandler<EntityDiedAfterEvent>
    {
        private readonly IGameManager _gameManager;
        private readonly ILocalizer _localizer;

        public NetworkLocation Location => NetworkLocation.Client;

        public Timing Timing => Timing.Main;

        public EntityDiedEventDefaultHandler(IGameManager gameManager, ILocalizer localizer)
        {
            _gameManager = gameManager;
            _localizer = localizer;
        }

        public UniTask<HandlerResult> HandleEventAsync(EntityDiedAfterEvent eventArgs)
        {
            _gameManager.GetUserInterface().HudOverlay.ShowAlert(GetDeathMessage(eventArgs));
            return UniTask.FromResult(new HandlerResult());
        }

        private string GetDeathMessage(EntityDiedAfterEvent eventArgs)
        {
            var victimName = eventArgs.EntityName;

            if (eventArgs.OwnerClientId == NetworkManager.Singleton.LocalClientId)
            {
                if (eventArgs.LastDamageSourceName == victimName)
                {
                    return eventArgs.LastDamageItemName.IsNullOrWhiteSpace()
                        ? _localizer.Translate("ui.alert.attack.youkilledyourself")
                        : _localizer.Translate("ui.alert.attack.youkilledyourselfusing", eventArgs.LastDamageItemName);
                }

                return eventArgs.LastDamageItemName.IsNullOrWhiteSpace()
                    ? _localizer.Translate("ui.alert.attack.youwerekilledby", eventArgs.LastDamageSourceName)
                    : _localizer.Translate("ui.alert.attack.youwerekilledbyusing", eventArgs.LastDamageSourceName, eventArgs.LastDamageItemName);
            }

            if (eventArgs.LastDamageSourceName == victimName)
            {
                return eventArgs.LastDamageItemName.IsNullOrWhiteSpace()
                    ? _localizer.Translate("ui.alert.attack.victimsuicide", victimName)
                    : _localizer.Translate("ui.alert.attack.victimsuicideusing", victimName, eventArgs.LastDamageItemName);
            }

            return eventArgs.LastDamageItemName.IsNullOrWhiteSpace()
                ? _localizer.Translate("ui.alert.attack.victimkilledby", victimName, eventArgs.LastDamageSourceName)
                : _localizer.Translate("ui.alert.attack.victimkilledbyusing", victimName, eventArgs.LastDamageSourceName, eventArgs.LastDamageItemName);
        }
    }
}
