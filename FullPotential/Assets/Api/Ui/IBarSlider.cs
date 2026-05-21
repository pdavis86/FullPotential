// ReSharper disable UnusedMemberInSuper.Global

namespace FullPotential.Api.Ui.Components
{
    public interface IBarSlider
    {
        void UpdateValues(string text, float value);

        void UpdateValues(string text, float value, float maxValue);
    }
}
