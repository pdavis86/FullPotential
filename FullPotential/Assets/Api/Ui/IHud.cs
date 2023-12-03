using FullPotential.Api.Gameplay.Behaviours;
using UnityEngine;

// ReSharper disable UnusedMemberInSuper.Global

namespace FullPotential.Api.Ui
{
    public interface IHud
    {
        void Initialise(FighterBase fighter);

        void ShowAlert(string content);

        void ToggleDrawingMode(bool isOn);

        (float percent, string text) GetSliderBarValues(float currentValue, float maxValue, string extra);

        void AddSliderBar(string id, Color color);

        void UpdateSliderBar(string id, string text, float value, float maxValue);

        void RemoveSliderBar(string id);

        void AddHandIcon(string id, bool isLeftHand, GameObject prefab);

        void RemoveHandIcon(string id);
    }
}
