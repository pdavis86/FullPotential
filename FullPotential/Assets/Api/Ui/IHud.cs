using UnityEngine;

// ReSharper disable UnusedMemberInSuper.Global

namespace FullPotential.Api.Ui
{
    public interface IHud
    {
        void ShowAlert(string content);

        void ToggleDrawingMode(bool isOn);

        (float percent, string text) GetSliderBarValues(float currentValue, float maxValue, string extra);

        void UpdateSliderBar(string id, string text, float value, float maxValue);

        void ToggleSliderBar(string id, bool show);

        void AddHandIcon(string id, string slotId, GameObject prefab);

        void RemoveHandIcon(string id);
    }
}
