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

        private bool _isDestroySet;

        public void SetEffect(Guid id, string effectTranslation, float timeToLive, Color color)
        {
            if (_isDestroySet)
            {
                CancelInvoke(nameof(DestroyMe));
            }

            Id = id;

            _image.color = color;

            _text.text = effectTranslation + $" ({timeToLive:F1}s)";

            Invoke(nameof(DestroyMe), timeToLive);
            _isDestroySet = true;
        }

        private void DestroyMe()
        {
            Destroy(gameObject);
        }

    }
}
