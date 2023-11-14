using FullPotential.Api.Ioc;
using FullPotential.Api.Localization;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FullPotential.Core.Gameplay.Tooltips
{
    public class TooltipWithTranslation : Tooltip
    {
        private string _translation;

#pragma warning disable CS0649
        [SerializeField] private string _translationKey;
#pragma warning restore CS0649

        // ReSharper disable once UnusedMember.Global
        public void Start()
        {
            var localizer = DependenciesContext.Dependencies.GetService<ILocalizer>();
            _translation = localizer.Translate(_translationKey);

            OnPointerEnterForTooltip += ShowTooltip;
        }

        // ReSharper disable once UnusedMember.Global
        public void OnDestroy()
        {
            OnPointerEnterForTooltip -= ShowTooltip;
        }

        private void ShowTooltip(PointerEventData eventData)
        {
            Tooltips.ShowTooltip(_translation);
        }
    }
}
