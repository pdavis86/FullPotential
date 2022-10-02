using FullPotential.Api.Registry.Effects;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable once ClassNeverInstantiated.Global

namespace FullPotential.Core.UI.Behaviours
{
    public class ActiveEffect : MonoBehaviour
    {
        #region Inspector Variables
        // ReSharper disable UnassignedField.Compiler

        [SerializeField] private Image _image;
        [SerializeField] private Text _text;

        // ReSharper restore UnassignedField.Compiler
        #endregion

        public IEffect Effect { get; private set; }

        private bool _isDestroySet;

        public void SetEffect(IEffect effect, string effectTranslation, float timeToLive, Color color)
        {
            if (_isDestroySet)
            {
                CancelInvoke(nameof(DestroyMe));
            }

            Effect = effect;

            _image.color = color;

            _text.text = effectTranslation + $" ({timeToLive}s)";

            Invoke(nameof(DestroyMe), timeToLive);
            _isDestroySet = true;
        }

        private void DestroyMe()
        {
            Destroy(gameObject);
        }

    }
}
