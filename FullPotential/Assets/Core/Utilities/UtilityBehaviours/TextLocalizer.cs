using FullPotential.Api.Utilities.Extensions;
using FullPotential.Core.GameManagement;
using FullPotential.Core.Localization;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable once UnusedType.Global

namespace FullPotential.Core.Utilities.UtilityBehaviours
{
    [RequireComponent(typeof(Text))]
    public class TextLocalizer : MonoBehaviour
    {
        private Text _textComponent;
        private Localizer _localizer;

        // ReSharper disable once UnassignedField.Global
        public string TranslationId;

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            _textComponent = GetComponent<Text>();
            _localizer = GameManager.Instance.GetService<Localizer>();
        }

        // ReSharper disable once UnusedMember.Local
        private void OnEnable()
        {
            if (TranslationId.IsNullOrWhiteSpace())
            {
                Debug.LogWarning($"Missing {nameof(TranslationId)} on {gameObject.name} under {transform.parent.gameObject.name}");
            }

            _textComponent.text = _localizer.Translate(TranslationId);
        }

    }
}
