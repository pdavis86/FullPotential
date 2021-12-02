﻿using System;

// ReSharper disable InconsistentNaming

namespace FullPotential.Core.Data
{
    [Serializable]
    public struct KeyValuePair<K, V>
    {
        public K Key;
        public V Value;

        public KeyValuePair(K key, V value)
        {
            Key = key;
            Value = value;
        }
    }
}
