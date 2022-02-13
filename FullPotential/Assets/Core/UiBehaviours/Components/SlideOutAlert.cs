using UnityEngine;

// ReSharper disable once UnusedType.Global

namespace FullPotential.Core.UiBehaviours.Components
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