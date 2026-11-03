using System;

namespace FullPotential.Api.Gameplay.Attributes
{
    public readonly struct AttributeTypeId : IEquatable<AttributeTypeId>
    {
        private readonly Guid _id;

        public AttributeTypeId(Guid id)
        {
            _id = id;
        }

        public static AttributeTypeId FromString(string value)
        {
            if (!Guid.TryParse(value, out var id))
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("An attribute type ID cannot be null or empty.", nameof(value));
                }

                throw new FormatException($"'{value}' is not a valid attribute type ID.");
            }

            return new AttributeTypeId(id);
        }

        public static bool TryParse(string value, out AttributeTypeId attributeTypeId)
        {
            attributeTypeId = default;
            if (!Guid.TryParse(value, out var id))
            {
                return false;
            }

            attributeTypeId = new AttributeTypeId(id);
            return true;
        }

        public bool Equals(AttributeTypeId other) => _id == other._id;

        public override bool Equals(object obj) => obj is AttributeTypeId other && Equals(other);

        public override int GetHashCode() => _id.GetHashCode();

        public override string ToString() => _id.ToString();

        public static bool operator ==(AttributeTypeId left, AttributeTypeId right) => left.Equals(right);

        public static bool operator !=(AttributeTypeId left, AttributeTypeId right) => !left.Equals(right);
    }
}
