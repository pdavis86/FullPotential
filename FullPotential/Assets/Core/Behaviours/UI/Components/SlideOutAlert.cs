using UnityEngine;

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedType.Global

namespace FullPotential.Core.Behaviours.Ui.Components
{
    public class SlideOutAlert : MonoBehaviour
    {
        private void Start()
        {
            Destroy(gameObject, 3f);
        }
    }
}