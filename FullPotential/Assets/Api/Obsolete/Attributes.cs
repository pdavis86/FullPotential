namespace FullPotential.Api.Obsolete
{
    [System.Serializable]
    public struct Attributes
    {
        //todo: zzz v0.5 - generalise these after moving to use a database
        //i.e. dictionary of attributes
        //so it is easier to add attributes in the future or in mods

        public bool IsSoulbound;
        public bool IsAutomatic;
        public byte ExtraAmmoPerShot;

        public int Strength;
        public int Efficiency;
        public int Range;
        public int Accuracy;
        public int Speed;
        public int Recovery;
        public int Duration;
        public int Luck;

        public int GetNameHash()
        {
            var hash = 101;
            hash = hash * 103 + IsSoulbound.GetHashCode();
            hash = hash * 107 + IsAutomatic.GetHashCode();
            hash = hash * 109 + ExtraAmmoPerShot.GetHashCode();

            hash = hash * 113 + Strength.GetHashCode();
            hash = hash * 127 + Efficiency.GetHashCode();
            hash = hash * 131 + Range.GetHashCode();
            hash = hash * 137 + Accuracy.GetHashCode();
            hash = hash * 139 + Speed.GetHashCode();
            hash = hash * 149 + Recovery.GetHashCode();
            hash = hash * 151 + Duration.GetHashCode();
            hash = hash * 157 + Luck.GetHashCode();

            return hash;
        }

    }
}
