using System;

// ReSharper disable InconsistentNaming

namespace FullPotential.Api.Data
{
    [Serializable]
    public struct SerializableKeyValuePair<K, V>
    {
        public K Key;
        public V Value;

        public SerializableKeyValuePair(K key, V value)
        {
            Key = key;
            Value = value;
        }
    }
}
