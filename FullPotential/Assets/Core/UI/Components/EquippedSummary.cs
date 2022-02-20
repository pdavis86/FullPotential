using FullPotential.Api.Utilities.Extensions;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.Ui.Components
{
    public class EquippedSummary : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField] private Image _image;
        [SerializeField] private Text _text;
#pragma warning restore CS0649

        // ReSharper disable once UnusedMember.Local
        private void Start()
        {
            SetTransparency(0.25f);
        }

        private void SetTransparency(float alpha)
        {
            _image.color = new Color(_image.color.r, _image.color.g, _image.color.b,  alpha);
            _text.color = new Color(_text.color.r, _text.color.g, _text.color.b, alpha);
        }

        public void SetContents(string contents)
        {
            if (contents.IsNullOrWhiteSpace())
            {
                gameObject.SetActive(false);
                return;
            }

            _text.text = contents.Trim();

            gameObject.SetActive(true);
        }
    }
}
