using UnityEngine;

// ReSharper disable UnusedMemberInSuper.Global

namespace FullPotential.Api.Ui
{
    public interface IHud
    {
        // todo: make all of these redundant

        void ShowAlert(string content);

        void ToggleDrawingMode(bool isOn);

        void UpdateSliderBar(string id, float value, float maxValue);

        void ToggleSliderBar(string id, bool show);

        void AddHandIcon(string id, string slotId, GameObject prefab);

        void RemoveHandIcon(string id);
    }
}
