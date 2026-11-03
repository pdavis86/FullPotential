using FullPotential.Api.CoreTypeIds;

using UnityEngine;

namespace FullPotential.Api.Ui
{
    public interface IHud
    {
        void ToggleDrawingMode(bool isOn);

        void AddHandIcon(string id, SlotId slotId, GameObject prefab);

        void RemoveHandIcon(string id);
    }
}
