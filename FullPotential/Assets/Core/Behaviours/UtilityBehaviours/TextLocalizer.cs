using UnityEngine;
using UnityEngine.UI;

// ReSharper disable CheckNamespace
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable UnassignedField.Global

[RequireComponent(typeof(Text))]
public class TextLocalizer : MonoBehaviour
{
    private Text _textComponent;

    public string TranslationId;

    private void Awake()
    {
        _textComponent = GetComponent<Text>();
    }

    private void OnEnable()
    {
        _textComponent.text = GameManager.Instance.Localizer.Translate(TranslationId);
    }

}
