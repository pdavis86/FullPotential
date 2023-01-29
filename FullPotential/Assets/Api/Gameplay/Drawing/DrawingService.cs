using System;
using FullPotential.Api.Utilities.Extensions;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Api.Gameplay.Drawing
{
    public class DrawingService : IDrawingService
    {
        public string GetDrawingCode(DrawShape drawShape, Vector2? direction = null)
        {
            if (drawShape == DrawShape.Circle)
            {
                return "Circle";
            }

            if (direction == null)
            {
                throw new Exception("Direction is required");
            }

            var drawingCode = "Line:";

            var angle = Vector2.up.ClockwiseAngleTo(direction.Value);

            if (angle >= 337.5 || angle < 22.5)
            {
                drawingCode += "v";
            }
            else if (angle >= 22.5 && angle < 67.5)
            {
                drawingCode += "ru";
            }
            else if (angle >= 67.5 && angle < 112.5)
            {
                drawingCode += "h";
            }
            else if (angle >= 112.5 && angle < 157.5)
            {
                drawingCode += "rd";
            }
            else if (angle >= 157.5 && angle < 202.5)
            {
                drawingCode += "v";
            }
            else if (angle >= 202.5 && angle < 247.5)
            {
                drawingCode += "ld";
            }
            else if (angle >= 247.5 && angle < 292.5)
            {
                drawingCode += "h";
            }
            else if (angle >= 292.5 && angle < 337.5)
            {
                drawingCode += "lu";
            }

            return drawingCode;
        }
    }
}
