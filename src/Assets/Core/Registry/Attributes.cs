
// ReSharper disable NonReadonlyMemberInGetHashCode

namespace Assets.Core.Registry
{
    [System.Serializable]
    public struct Attributes
    {
        public bool IsAutomatic;
        public bool IsSoulbound;
        public int ExtraAmmoPerShot;
        public int Strength;
        public int Efficiency;
        public int Range;
        public int Accuracy;
        public int Speed;
        public int Recovery;
        public int Duration;
        public int Luck;

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 101;
                hash = hash * 103 + IsAutomatic.GetHashCode();
                hash = hash * 107 + IsSoulbound.GetHashCode();
                
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
}
