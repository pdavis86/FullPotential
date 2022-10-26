using UnityEngine;
using UnityEngine.UI;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.Ui.Components
{
    public class SlideOutAlert : MonoBehaviour
    {
        // ReSharper disable UnassignedField.Global
        public Text Text;
        // ReSharper restore UnassignedField.Global

        // ReSharper disable once UnusedMember.Local
        private void Start()
        {
            Destroy(gameObject, 3f);
        }
    }
}