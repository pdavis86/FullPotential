using FullPotential.Api.Gameplay.Combat;

namespace FullPotential.Api.Ui
{
    public interface IHud
    {
        void Initialise(FighterBase fighter);

        void ShowAlert(string content);

        void ToggleCursorCapture(bool isOn);
    }
}
