using UnityEngine;

namespace FullPotential.Api.Gameplay.Drawing
{
    public interface IDrawingService
    {
        string GetDrawingCode(Vector2 direction, int length);
    }
}
