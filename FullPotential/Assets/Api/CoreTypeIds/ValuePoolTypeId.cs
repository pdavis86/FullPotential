using System;

namespace FullPotential.Api.CoreTypeIds
{
    public readonly struct ValuePoolTypeId : IEquatable<ValuePoolTypeId>
    {
        private readonly Guid _id;

        public ValuePoolTypeId(Guid id)
        {
            _id = id;
        }

        public static ValuePoolTypeId FromString(string value)
        {
            if (!Guid.TryParse(value, out var id))
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("A value pool type ID cannot be null or empty.", nameof(value));
                }

                throw new FormatException($"'{value}' is not a valid value pool type ID.");
            }

            return new ValuePoolTypeId(id);
        }

        public static bool TryParse(string value, out ValuePoolTypeId valuePoolTypeId)
        {
            valuePoolTypeId = default;
            if (!Guid.TryParse(value, out var id))
            {
                return false;
            }

            valuePoolTypeId = new ValuePoolTypeId(id);
            return true;
        }

        public bool Equals(ValuePoolTypeId other) => _id == other._id;
        public override bool Equals(object obj) => obj is ValuePoolTypeId other && Equals(other);
        public override int GetHashCode() => _id.GetHashCode();
        public override string ToString() => _id.ToString();
        public static implicit operator ValuePoolTypeId(string value) => FromString(value);
        public static implicit operator string(ValuePoolTypeId valuePoolTypeId) => valuePoolTypeId.ToString();
        public static bool operator ==(ValuePoolTypeId left, ValuePoolTypeId right) => left.Equals(right);
        public static bool operator !=(ValuePoolTypeId left, ValuePoolTypeId right) => !left.Equals(right);
    }
}
