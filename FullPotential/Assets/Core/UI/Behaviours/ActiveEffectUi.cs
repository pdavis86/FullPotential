using System;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.UI.Behaviours
{
    public class ActiveEffectUi : MonoBehaviour
    {
        #region Inspector Variables
        // ReSharper disable UnassignedField.Compiler

        [SerializeField] private Image _image;
        [SerializeField] private Text _text;

        // ReSharper restore UnassignedField.Compiler
        #endregion

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

            UpdateEffect(expiry);
        }

        public void UpdateEffect(DateTime expiry)
        {
            var secondsRemaining = (float)(expiry - DateTime.Now).TotalSeconds;

            if (expiry != _expiry)
            {
                _expiry = expiry;
                DestroyAfter(Math.Max(secondsRemaining, 2));
            }

            if (_showExpiry)
            {
                _text.text = _effectTranslation + $" ({secondsRemaining:F1}s)";
            }
            else
            {
                _text.text = _effectTranslation;
            }
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
