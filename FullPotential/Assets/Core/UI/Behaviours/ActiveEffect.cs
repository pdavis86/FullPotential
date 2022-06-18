using FullPotential.Api.Registry.Effects;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable once ClassNeverInstantiated.Global

namespace FullPotential.Core.UI.Behaviours
{
    public class ActiveEffect : MonoBehaviour
    {
        // ReSharper disable MemberCanBePrivate.Global
        // ReSharper disable UnassignedField.Global
        public Image Image;
        public Text Text;
        // ReSharper enable UnassignedField.Global
        // ReSharper enable MemberCanBePrivate.Global

        public IEffect Effect { get; private set; }

        private bool _isDestroySet;

        public void SetEffect(IEffect effect, string effectTranslation, float timeToLive)
        {
            if (_isDestroySet)
            {
                CancelInvoke(nameof(DestroyMe));
            }

            Effect = effect;

            //todo: activeEffectScript.Image = 

            Text.text = effectTranslation;

            Invoke(nameof(DestroyMe), timeToLive);
            _isDestroySet = true;
        }

        private void DestroyMe()
        {
            Destroy(gameObject);
        }

    }
}
