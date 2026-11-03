using System;

namespace FullPotential.Api.Registry.Weapons
{
    public readonly struct AmmunitionTypeId : IEquatable<AmmunitionTypeId>
    {
        private readonly Guid _id;

        public AmmunitionTypeId(Guid id)
        {
            _id = id;
        }

        public static AmmunitionTypeId FromString(string value)
        {
            if (!Guid.TryParse(value, out var id))
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("An ammunition type ID cannot be null or empty.", nameof(value));
                }

                throw new FormatException($"'{value}' is not a valid ammunition type ID.");
            }

            return new AmmunitionTypeId(id);
        }

        public static bool TryParse(string value, out AmmunitionTypeId ammunitionTypeId)
        {
            ammunitionTypeId = default;
            if (!Guid.TryParse(value, out var id))
            {
                return false;
            }

            ammunitionTypeId = new AmmunitionTypeId(id);
            return true;
        }

        public bool Equals(AmmunitionTypeId other) => _id == other._id;
        public override bool Equals(object obj) => obj is AmmunitionTypeId other && Equals(other);
        public override int GetHashCode() => _id.GetHashCode();
        public override string ToString() => _id.ToString();
        public static implicit operator AmmunitionTypeId(string value) => FromString(value);
        public static implicit operator string(AmmunitionTypeId ammunitionTypeId) => ammunitionTypeId.ToString();
        public static bool operator ==(AmmunitionTypeId left, AmmunitionTypeId right) => left.Equals(right);
        public static bool operator !=(AmmunitionTypeId left, AmmunitionTypeId right) => !left.Equals(right);
    }
}
