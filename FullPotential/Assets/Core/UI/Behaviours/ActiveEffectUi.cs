using System;

using UnityEngine;
using UnityEngine.UI;

namespace FullPotential.Core.UI.Behaviours
{
    public class ActiveEffectUi : MonoBehaviour
    {
        [SerializeField] private Image _image;
        [SerializeField] private Text _text;

        public Guid Id { get; private set; }

        private string _effectTranslation;
        private bool _showExpiry;
        private bool _isDestroySet;
        private DateTime _expiry;

        public void SetEffect(Guid id, Color color, string effectTranslation, bool showExpiry, DateTime expiry)
        {
            Id = id;
            _image.color = color;
            _effectTranslation = effectTranslation;
            _showExpiry = showExpiry;
            _expiry = expiry;

            DestroyAfter(Math.Max(GetSecondsRemaining(), 2));

            UpdateEffect();
        }

        public void UpdateEffect()
        {
            if (_text.IsDestroyed())
            {
                return;
            }

            _text.text = _showExpiry
                ? _effectTranslation + $" ({GetSecondsRemaining():F1}s)"
                : _effectTranslation;
        }

        public void UpdateExpiry(DateTime expiry)
        {
            _expiry = expiry;
            DestroyAfter(Math.Max(GetSecondsRemaining(), 2));
        }

        public float GetSecondsRemaining()
        {
            return (float)(_expiry - DateTime.Now).TotalSeconds;
        }

        private void DestroyAfter(float timeToLive)
        {
            if (_isDestroySet)
            {
                CancelInvoke(nameof(DestroyMe));
            }

            Invoke(nameof(DestroyMe), timeToLive);
            _isDestroySet = true;
        }

        private void DestroyMe()
        {
            Destroy(gameObject);
        }

    }
}
