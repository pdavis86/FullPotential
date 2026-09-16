using System.Linq;

using FullPotential.Api.Ioc;
using FullPotential.Api.Localization;
using FullPotential.Api.Logging;
using FullPotential.Api.Utilities.Extensions;

using UnityEngine;
using UnityEngine.UI;

// ReSharper disable UnusedType.Global

namespace FullPotential.Core.Utilities.UtilityBehaviours
{
    [RequireComponent(typeof(Text))]
    public class TextLocalizer : MonoBehaviour
    {
        private Text _textComponent;

        private IAuditor _logger;
        private ILocalizer _localizer;

        // ReSharper disable once UnassignedField.Global
        public string TranslationId;

        public string[] Arguments;

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            _textComponent = GetComponent<Text>();

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

            var baseTranslation = _localizer.Translate(TranslationId);

            if (Arguments != null && Arguments.Any())
            {
                // ReSharper disable once CoVariantArrayConversion
                _textComponent.text = string.Format(baseTranslation, Arguments);
            }
            else
            {
                _textComponent.text = baseTranslation;
            }
        }

    }
}
