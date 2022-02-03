using FullPotential.Core.Behaviours.GameManagement;
using FullPotential.Core.Extensions;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable once UnusedType.Global

namespace FullPotential.Core.Behaviours.UtilityBehaviours
{
    [RequireComponent(typeof(Text))]
    public class TextLocalizer : MonoBehaviour
    {
        private Text _textComponent;

        // ReSharper disable once UnassignedField.Global
        public string TranslationId;

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            _textComponent = GetComponent<Text>();
        }

        // ReSharper disable once UnusedMember.Local
        private void OnEnable()
        {
            if (TranslationId.IsNullOrWhiteSpace())
            {
                Debug.LogWarning($"Missing {nameof(TranslationId)} on {gameObject.name} under {transform.parent.gameObject.name}");
            }

            _textComponent.text = GameManager.Instance.Localizer.Translate(TranslationId);
        }

    }
}
