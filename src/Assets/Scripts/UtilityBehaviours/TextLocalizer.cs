using UnityEngine;
using UnityEngine.UI;

// ReSharper disable CheckNamespace
// ReSharper disable UnusedType.Global

[RequireComponent(typeof(Text))]
public class TextLocalizer : MonoBehaviour
{
    public string id;

    void Start()
    {
        GetComponent<Text>().text = ResolveStringValue(id);
    }

    void OnValidate()
    {
        GetComponent<Text>().text = ResolveStringValue(id);
    }

    public string ResolveStringValue(string id)
    {
        //todo: based on set language and ID, return the translation
        return id;
    }

}
