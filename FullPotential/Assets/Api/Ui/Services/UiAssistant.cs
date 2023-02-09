using FullPotential.Api.Utilities.Extensions;
using UnityEngine;
using UnityEngine.EventSystems;

// ReSharper disable once ClassNeverInstantiated.Global

namespace FullPotential.Api.Ui.Services
{
    public class UiAssistant : IUiAssistant
    {
        private readonly EventSystem _currentEventSystem;

        public UiAssistant()
        {
            _currentEventSystem = EventSystem.current;
        }

        public void SelectNextGameObject(GameObject[] sequence, bool forwards)
        {
            if (sequence.Length < 2)
            {
                return;
            }

            var currentObj = _currentEventSystem.currentSelectedGameObject;
            var currentIndex = sequence.IndexOf(currentObj);

            int nextIndex;

            if (forwards)
            {
                nextIndex = currentIndex + 1 < sequence.Length
                    ? currentIndex + 1
                    : 0;
            }
            else
            {
                nextIndex = currentIndex - 1 >= 0
                    ? currentIndex - 1
                    : sequence.Length - 1;
            }

            var nextObj =  sequence[nextIndex];

            if (nextObj == null)
            {
                return;
            }

            _currentEventSystem.SetSelectedGameObject(nextObj);
        }
    }
}
