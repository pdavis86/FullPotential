using FullPotential.Core.Behaviours.GameManagement;
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
            _textComponent.text = GameManager.Instance.Localizer.Translate(TranslationId);
        }

    }
}
