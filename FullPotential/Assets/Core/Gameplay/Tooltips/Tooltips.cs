using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


namespace FullPotential.Core.Gameplay.Tooltips
{
    public class Tooltips : MonoBehaviour
    {
        private static Tooltips _instance;

        private Text _tooltipText;
        private RectTransform _rect;
        private Vector3 _underOffset;
        private Vector3 _leftOffset;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this);
                return;
            }
            _instance = this;

            _tooltipText = transform.Find("Text").GetComponent<Text>();
            _rect = GetComponent<RectTransform>();
            gameObject.SetActive(false);
        }

        private void FixedUpdate()
        {
            if (gameObject.activeSelf)
            {
                var pointerPosition = Mouse.current.position.ReadValue();

                var underPointer = (pointerPosition.y + _rect.sizeDelta.y > Screen.height) && _rect.sizeDelta.y < Screen.height;
                var leftOfPointer = (pointerPosition.x + _rect.sizeDelta.x > Screen.width) && _rect.sizeDelta.x < Screen.width;

                transform.position = new Vector3(pointerPosition.x, pointerPosition.y) +
                    (underPointer ? _underOffset : new Vector3(0, 1))
                    + (leftOfPointer ? _leftOffset : new Vector3(1, 0));
            }
        }

        public void Show(string tooltipText)
        {
            const int padding = 15;

            gameObject.SetActive(true);
            _tooltipText.text = tooltipText;
            _rect.sizeDelta = new Vector2(_tooltipText.preferredWidth + padding, _tooltipText.preferredHeight + padding);

            _underOffset = new Vector3(1, -_rect.sizeDelta.y - 1);
            _leftOffset = new Vector3(-_rect.sizeDelta.x - 1, 1);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public static void ShowTooltip(string tooltipText)
        {
            if (!string.IsNullOrWhiteSpace(tooltipText))
            {
                _instance.Show(tooltipText);
            }
        }

        public static void HideTooltip()
        {
            _instance.Hide();
        }

    }
}
