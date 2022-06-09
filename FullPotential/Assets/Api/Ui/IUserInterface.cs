using UnityEngine;

namespace FullPotential.Api.Ui
{
    public interface IUserInterface
    {
        IHud HudOverlay { get; }

        GameObject InteractionBubbleOverlay { get; }

        void OpenCustomMenu(GameObject menuGameObject);

        void SpawnProjectileTrail(Vector3 startPosition, Vector3 endPosition);
    }
}
