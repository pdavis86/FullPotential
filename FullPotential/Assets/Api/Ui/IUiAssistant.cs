using UnityEngine;

namespace FullPotential.Api.Ui
{
    public interface IUiAssistant
    {
        void SelectNextGameObject(GameObject[] sequence, bool forwards);
    }
}