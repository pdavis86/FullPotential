using System.Collections.Generic;

namespace Assets.Core
{
    public class Shape
    {
        public const string Zone = "Zone";
        public const string Wall = "Wall";

        public static readonly List<string> All = new List<string>
        {
            Zone,
            Wall
        };
    }
}
