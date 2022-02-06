namespace FullPotential.Api.Ui.Components
{
    public interface IStatSlider
    {
        void SetValues((float percent, string text) values);

        (float percent, string text) GetStaminaValues(int stamina, int maxStamina);

        (float percent, string text) GetHealthValues(int health, int maxHealth, int defence);

        (float percent, string text) GetManaValues(int mana, int maxMana);
    }
}
