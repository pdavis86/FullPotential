using System.Text;
using FullPotential.Api.Gameplay.Drawing;
using FullPotential.Api.Unity.Extensions;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.Gameplay.Drawing
{
    public class DrawingService : IDrawingService
    {
        public string GetDrawingCode(Vector2 direction, int length)
        {
            var builder = new StringBuilder();

            var angle = Vector2.up.ClockwiseAngleTo(direction);

            if (angle >= 337.5 || angle < 22.5)
            {
                builder.Append("vu");
            }
            else if (angle >= 22.5 && angle < 67.5)
            {
                builder.Append("ru");
            }
            else if (angle >= 67.5 && angle < 112.5)
            {
                builder.Append("hr");
            }
            else if (angle >= 112.5 && angle < 157.5)
            {
                builder.Append("rd");
            }
            else if (angle >= 157.5 && angle < 202.5)
            {
                builder.Append("vd");
            }
            else if (angle >= 202.5 && angle < 247.5)
            {
                builder.Append("ld");
            }
            else if (angle >= 247.5 && angle < 292.5)
            {
                builder.Append("hl");
            }
            else if (angle >= 292.5 && angle < 337.5)
            {
                builder.Append("lu");
            }

            builder.Append($":{length}");

            return builder.ToString();
        }
    }
}
