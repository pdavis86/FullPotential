using FullPotential.Api.Data;

namespace FullPotential.Api.Ui
{
    public interface IHud
    {
        void ShowAlert(string content);

        void UpdateHand(string contents, bool isLeftHand);

        void UpdateAmmo(bool isLeftHand, PlayerHandStatus playerHandStatus);

        void UpdateStaminaPercentage(int stamina, int maxStamina);

        void UpdateHealthPercentage(int health, int maxHealth, int defence);

        void UpdateManaPercentage(int mana, int maxMana);

        void ToggleCursorCapture(bool isOn);
    }
}
