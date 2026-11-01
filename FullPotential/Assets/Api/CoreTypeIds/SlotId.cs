using System;

namespace FullPotential.Api.CoreTypeIds
{
    public readonly struct SlotId : IEquatable<SlotId>
    {
        private readonly string _idString;

        public SlotId(Guid id)
        {
            _idString = id.ToString();
        }

        public SlotId(Guid id, int index)
        {
            _idString = $"{id};{index}";
        }

        public static SlotId FromString(string value)
        {
            if (!TryParse(value, out var slotId))
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("A slot ID cannot be null or empty.", nameof(value));
                }

                throw new FormatException($"'{value}' is not a valid slot ID.");
            }

            return slotId;
        }

        public static bool TryParse(string value, out SlotId slotId)
        {
            slotId = default;

            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            var parts = value.Split(';');
            if (parts.Length == 1 && Guid.TryParse(parts[0], out var id))
            {
                slotId = new SlotId(id);
                return true;
            }

            if (parts.Length == 2
                && Guid.TryParse(parts[0], out id)
                && int.TryParse(parts[1], out var index))
            {
                slotId = new SlotId(id, index);
                return true;
            }

            return false;
        }

        public bool Equals(SlotId other) => _idString == other._idString;

        public override bool Equals(object obj) => obj is SlotId other && Equals(other);

        public override int GetHashCode() => _idString == null ? 0 : _idString.GetHashCode();

        public override string ToString() => _idString;

        public static bool operator ==(SlotId left, SlotId right) => left.Equals(right);

        public static bool operator !=(SlotId left, SlotId right) => !left.Equals(right);
    }
}
