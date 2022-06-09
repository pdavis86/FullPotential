using UnityEngine;

namespace FullPotential.Api.Ui
{
    public interface IUserInterface
    {
        IHud HudOverlay { get; }

        GameObject InteractionBubbleOverlay { get; }

        void OpenCustomMenu(GameObject menuGameObject);
    }
}
