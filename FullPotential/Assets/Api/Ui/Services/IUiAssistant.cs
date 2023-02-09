using UnityEngine;

namespace FullPotential.Api.Ui.Services
{
    public interface IUiAssistant
    {
        void SelectNextGameObject(GameObject[] sequence, bool forwards);
    }
}