using System;

namespace FullPotential.Api.Registry.Visuals
{
    public readonly struct ItemVisualsTypeId : IEquatable<ItemVisualsTypeId>
    {
        private readonly Guid _id;

        public ItemVisualsTypeId(Guid id)
        {
            _id = id;
        }

        public static ItemVisualsTypeId FromString(string value)
        {
            if (!Guid.TryParse(value, out var id))
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("An item visuals type ID cannot be null or empty.", nameof(value));
                }

                throw new FormatException($"'{value}' is not a valid item visuals type ID.");
            }

            return new ItemVisualsTypeId(id);
        }

        public static bool TryParse(string value, out ItemVisualsTypeId itemVisualsTypeId)
        {
            itemVisualsTypeId = default;
            if (!Guid.TryParse(value, out var id))
            {
                return false;
            }

            itemVisualsTypeId = new ItemVisualsTypeId(id);
            return true;
        }

        public bool Equals(ItemVisualsTypeId other) => _id == other._id;
        public override bool Equals(object obj) => obj is ItemVisualsTypeId other && Equals(other);
        public override int GetHashCode() => _id.GetHashCode();
        public override string ToString() => _id.ToString();
        public static implicit operator ItemVisualsTypeId(string value) => FromString(value);
        public static implicit operator string(ItemVisualsTypeId itemVisualsTypeId) => itemVisualsTypeId.ToString();
        public static bool operator ==(ItemVisualsTypeId left, ItemVisualsTypeId right) => left.Equals(right);
        public static bool operator !=(ItemVisualsTypeId left, ItemVisualsTypeId right) => !left.Equals(right);
    }
}
