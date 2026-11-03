using UnityEngine;


using FullPotential.Api.CoreTypeIds;

namespace FullPotential.Api.Ui
{
    public interface IHud
    {
        void ToggleDrawingMode(bool isOn);

        void AddHandIcon(string id, SlotId slotId, GameObject prefab);

        void RemoveHandIcon(string id);
    }
}
