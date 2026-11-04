namespace FullPotential.Api.Unity.Extensions
{
    public static class UnitsExtensions
    {
        public static float ToMetres(this float unityUnits)
        {
            //Rough translation from Unity units to metres
            return unityUnits * 0.6f;
        }
    }
}
