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
    private bool _hasStarted;

    public string TranslationId;
    public bool FireOnValidate = false;

    void Awake()
    {
        _textComponent = GetComponent<Text>();
    }

    void Start()
    {
        if (GameManager.Instance == null)
        {
            return;
        }
        ResolveStringValue(TranslationId);
        _hasStarted = true;
    }

    void OnValidate()
    {
        if (_hasStarted && FireOnValidate)
        {
            ResolveStringValue(TranslationId);
        }
    }

    void ResolveStringValue(string id)
    {
        _textComponent.text = GameManager.Instance.Localizer.Translate(id);
    }

}
