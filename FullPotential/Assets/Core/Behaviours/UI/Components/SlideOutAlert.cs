using UnityEngine;

// ReSharper disable UnusedType.Global

namespace FullPotential.Core.Behaviours.Ui.Components
{
    public class SlideOutAlert : MonoBehaviour
    {
        // ReSharper disable once UnusedMember.Local
        private void Start()
        {
            Destroy(gameObject, 3f);
        }
    }
}