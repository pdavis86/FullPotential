using UnityEngine;
using UnityEngine.UI;

// ReSharper disable once UnusedType.Global

namespace FullPotential.Core.Ui.Components
{
    public class SlideOutAlert : MonoBehaviour
    {
        public Text Text;

        // ReSharper disable once UnusedMember.Local
        private void Start()
        {
            Destroy(gameObject, 3f);
        }
    }
}