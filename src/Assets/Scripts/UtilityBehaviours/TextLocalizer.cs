using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UtilityBehaviours
{
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
}
