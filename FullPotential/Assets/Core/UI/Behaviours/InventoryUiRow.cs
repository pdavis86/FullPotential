using TMPro;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.UI.Behaviours
{
    public class InventoryUiRow : MonoBehaviour
    {
        // ReSharper disable UnassignedField.Global
        public Text Text;
        public Button AssignedShapeButton;
        public TextMeshProUGUI AssignedShapeText;
        // ReSharper restore UnassignedField.Global

        public void ToggleButton(bool show)
        {
            AssignedShapeButton.gameObject.SetActive(show);
        }
    }
}
