namespace Assets.Scripts.Crafting.Results
{
    [System.Serializable]
    public struct Attributes
    {
        public bool IsActivated;
        public bool IsAutomatic;
        public bool IsSoulbound;
        public int ExtraAmmoPerShot;
        public int Strength;
        public int Cost;
        public int Range;
        public int Accuracy;
        public int Speed;
        public int Recovery;
        public int Duration;

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 101;
                hash = hash * 103 + IsActivated.GetHashCode();
                hash = hash * 107 + IsAutomatic.GetHashCode();
                hash = hash * 109 + IsSoulbound.GetHashCode();
                
                hash = hash * 113 + ExtraAmmoPerShot.GetHashCode();
                hash = hash * 127 + Strength.GetHashCode();
                hash = hash * 131 + Cost.GetHashCode();
                hash = hash * 137 + Range.GetHashCode();
                hash = hash * 139 + Accuracy.GetHashCode();
                hash = hash * 149 + Speed.GetHashCode();
                hash = hash * 151 + Recovery.GetHashCode();
                hash = hash * 157 + Duration.GetHashCode();

                return hash;
            }
        }

    }
}
