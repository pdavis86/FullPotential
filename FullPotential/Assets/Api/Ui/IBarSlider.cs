
// ReSharper disable UnusedMember.Global

namespace FullPotential.Api.Ui
{
    public interface IBarSlider
    {
        void UpdateValues(string text, float value);

        void UpdateValues(string text, float value, float maxValue);
    }
}
