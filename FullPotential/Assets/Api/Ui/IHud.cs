using UnityEngine;

// ReSharper disable UnusedMemberInSuper.Global

namespace FullPotential.Api.Ui
{
    public interface IHud
    {
        void ToggleDrawingMode(bool isOn);

        void AddHandIcon(string id, string slotId, GameObject prefab);

        void RemoveHandIcon(string id);
    }
}
