using System;

namespace FullPotential.Api.Registry.Effects
{
    public readonly struct EffectTypeId : IEquatable<EffectTypeId>
    {
        private readonly Guid _id;

        public EffectTypeId(Guid id)
        {
            _id = id;
        }

        public static EffectTypeId FromString(string value)
        {
            if (!Guid.TryParse(value, out var id))
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("An effect type ID cannot be null or empty.", nameof(value));
                }

                throw new FormatException($"'{value}' is not a valid effect type ID.");
            }

            return new EffectTypeId(id);
        }

        public static bool TryParse(string value, out EffectTypeId effectTypeId)
        {
            effectTypeId = default;
            if (!Guid.TryParse(value, out var id))
            {
                return false;
            }

            effectTypeId = new EffectTypeId(id);
            return true;
        }

        public bool Equals(EffectTypeId other) => _id == other._id;
        public override bool Equals(object obj) => obj is EffectTypeId other && Equals(other);
        public override int GetHashCode() => _id.GetHashCode();
        public override string ToString() => _id.ToString();
        public static implicit operator EffectTypeId(string value) => FromString(value);
        public static implicit operator string(EffectTypeId effectTypeId) => effectTypeId.ToString();
        public static bool operator ==(EffectTypeId left, EffectTypeId right) => left.Equals(right);
        public static bool operator !=(EffectTypeId left, EffectTypeId right) => !left.Equals(right);
    }
}
