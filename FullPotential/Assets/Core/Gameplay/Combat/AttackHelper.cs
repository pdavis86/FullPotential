using FullPotential.Api.Gameplay;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Gameplay.Enums;
using FullPotential.Api.Utilities.Extensions;
using FullPotential.Core.GameManagement;
using FullPotential.Core.Localization;

// ReSharper disable once ClassNeverInstantiated.Global

namespace FullPotential.Core.Gameplay.Combat
{
    public class AttackHelper : IAttackHelper
    {
        private readonly Localizer _localizer;

        public AttackHelper(
            Localizer localizer)
        {
            _localizer = localizer;
        }

        public void CheckIfOffTheMap(IDamageable damageable, float yValue)
        {
            if (damageable.AliveState != LivingEntityState.Dead && yValue < GameManager.Instance.GetSceneBehaviour().Attributes.LowestYValue)
            {
                damageable.HandleDeath(_localizer.Translate("ui.alert.falldamage"), null);
            }
        }

        public string GetDeathMessage(bool isOwner, string victimName, string killerName, string itemName)
        {
            if (itemName.IsNullOrWhiteSpace())
            {
                return isOwner
                    ? string.Format(_localizer.Translate("ui.alert.attack.youwerekilledby"), killerName)
                    : string.Format(_localizer.Translate("ui.alert.attack.victimkilledby"), victimName, killerName);
            }

            return isOwner
                ? string.Format(_localizer.Translate("ui.alert.attack.youwerekilledbyusing"), killerName, itemName)
                : string.Format(_localizer.Translate("ui.alert.attack.victimkilledbyusing"), victimName, killerName, itemName);
        }

    }
}
