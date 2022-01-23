using FullPotential.Core.Behaviours.GameManagement;
using FullPotential.Core.Extensions;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Global

namespace FullPotential.Core.Behaviours.UtilityBehaviours
{
    [RequireComponent(typeof(Text))]
    public class TextLocalizer : MonoBehaviour
    {
        private Text _textComponent;

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
