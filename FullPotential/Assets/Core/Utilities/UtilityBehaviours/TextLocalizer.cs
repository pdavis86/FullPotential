using System.Linq;

using FullPotential.Api.Ioc;
using FullPotential.Api.Localization;
using FullPotential.Api.Logging;
using FullPotential.Api.Utilities.Extensions;

using UnityEngine;
using UnityEngine.UI;

namespace FullPotential.Core.Utilities.UtilityBehaviours
{
    [RequireComponent(typeof(Text))]
    public class TextLocalizer : MonoBehaviour
    {
        private Text _textComponent;

        private IAuditor _logger;
        private ILocalizer _localizer;

        public string TranslationId;

        public string[] Arguments;

        private void Awake()
        {
            _textComponent = GetComponent<Text>();

            _logger = DependenciesContext.Dependencies.GetService<IAuditorFactory>().Create(this);
            _localizer = DependenciesContext.Dependencies.GetService<ILocalizer>();
        }

        private void OnEnable()
        {
            if (TranslationId.IsNullOrWhiteSpace())
            {
                _logger.Warn("Missing {0} on {1} under {2}", nameof(TranslationId), gameObject.name, transform.parent.gameObject.name);
            }

            var baseTranslation = _localizer.Translate(TranslationId);

            if (Arguments != null && Arguments.Any())
            {
                _textComponent.text = string.Format(baseTranslation, Arguments);
            }
            else
            {
                _textComponent.text = baseTranslation;
            }
        }

    }
}
