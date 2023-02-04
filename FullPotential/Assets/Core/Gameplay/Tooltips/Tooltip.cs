using UnityEngine;
using UnityEngine.EventSystems;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.Gameplay.Tooltips
{
    public class Tooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public delegate void PointerEnterEventDelegate(PointerEventData eventData);
        public event PointerEnterEventDelegate OnPointerEnterForTooltip;

        public void OnPointerEnter(PointerEventData eventData)
        {
            OnPointerEnterForTooltip?.Invoke(eventData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Tooltips.HideTooltip();
        }

        public void ClearHandlers()
        {
            if (OnPointerEnterForTooltip == null)
            {
                return;
            }

            foreach (var d in OnPointerEnterForTooltip.GetInvocationList())
            {
                OnPointerEnterForTooltip -= (PointerEnterEventDelegate)d;
            }
        }

    }
}
