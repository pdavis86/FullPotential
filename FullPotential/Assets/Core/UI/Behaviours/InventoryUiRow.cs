using TMPro;

using UnityEngine;
using UnityEngine.UI;


namespace FullPotential.Core.UI.Behaviours
{
    public class InventoryUiRow : MonoBehaviour
    {
        public Text Text;
        public Button AssignedShapeButton;
        public TextMeshProUGUI AssignedShapeText;

        public void ToggleButton(bool show)
        {
            AssignedShapeButton.gameObject.SetActive(show);
        }
    }
}
