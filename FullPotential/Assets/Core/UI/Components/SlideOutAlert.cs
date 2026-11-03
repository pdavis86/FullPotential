using UnityEngine;
using UnityEngine.UI;


namespace FullPotential.Core.Ui.Components
{
    public class SlideOutAlert : MonoBehaviour
    {
        public Text Text;

        private void Start()
        {
            Destroy(gameObject, 3f);
        }
    }
}