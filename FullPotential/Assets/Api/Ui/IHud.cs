using System.Collections.Generic;
using FullPotential.Api.Gameplay.Data;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Api.Ui
{
    public interface IHud
    {
        void ShowAlert(string content);

        void UpdateHandDescription(bool isLeftHand, string contents);

        void UpdateHandAmmo(bool isLeftHand, PlayerHandStatus playerHandStatus);

        void UpdateStaminaPercentage(int stamina, int maxStamina);

        void UpdateHealthPercentage(int health, int maxHealth, int defence);

        void UpdateManaPercentage(int mana, int maxMana);

        void UpdateEnergyPercentage(int energy, int maxEnergy);

        void ToggleCursorCapture(bool isOn);

        void UpdateActiveEffects(Dictionary<IEffect, float> activeEffects);
    }
}
