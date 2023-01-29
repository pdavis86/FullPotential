using UnityEngine;

namespace FullPotential.Api.Gameplay.Drawing
{
    public interface IDrawingService
    {
        string GetDrawingCode(DrawShape drawShape, Vector2? direction = null);
    }
}
