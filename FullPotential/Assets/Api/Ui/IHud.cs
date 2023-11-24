using FullPotential.Api.Gameplay.Behaviours;

// ReSharper disable UnusedMemberInSuper.Global

namespace FullPotential.Api.Ui
{
    public interface IHud
    {
        void Initialise(FighterBase fighter);

        void ShowAlert(string content);

        void ToggleDrawingMode(bool isOn);

        (float percent, string text) GetStaminaValues(int stamina, int maxStamina);

        (float percent, string text) GetHealthValues(int health, int maxHealth, int defence);

        (float percent, string text) GetManaValues(int mana, int maxMana);

        (float percent, string text) GetEnergyValues(int energy, int maxEnergy);

        void SetHandWarning(bool isLeftHand, bool isActive);
    }
}
