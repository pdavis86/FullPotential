using FullPotential.Api.Ioc;
using FullPotential.Api.Localization;
using FullPotential.Api.Logging;
using FullPotential.Api.Utilities.Extensions;
using TMPro;
using UnityEngine;

// ReSharper disable once UnusedType.Global

namespace FullPotential.Core.Utilities.UtilityBehaviours
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class TextMeshProLocalizer : MonoBehaviour
    {
        private TextMeshProUGUI _textComponent;

        private IAuditor _logger;
        private ILocalizer _localizer;

        // ReSharper disable once UnassignedField.Global
        public string TranslationId;

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            _textComponent = GetComponent<TextMeshProUGUI>();

            _logger = DependenciesContext.Dependencies.GetService<IAuditorFactory>().Create(this);
            _localizer = DependenciesContext.Dependencies.GetService<ILocalizer>();
        }

        // ReSharper disable once UnusedMember.Local
        private void OnEnable()
        {
            if (TranslationId.IsNullOrWhiteSpace())
            {
                _logger.Warn($"Missing {nameof(TranslationId)} on {gameObject.name} under {transform.parent.gameObject.name}");
            }

            _textComponent.text = _localizer.Translate(TranslationId);
        }
    }
}
