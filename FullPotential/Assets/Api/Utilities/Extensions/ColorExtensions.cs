namespace FullPotential.Api.Utilities.Extensions
{
    public static class ColorExtensions
    {
        public static UnityEngine.Color ToUnityColor(this System.Drawing.Color color)
        {
            return new UnityEngine.Color(
                color.R / 255f,
                color.G / 255f,
                color.B / 255f);
        }
    }
}
