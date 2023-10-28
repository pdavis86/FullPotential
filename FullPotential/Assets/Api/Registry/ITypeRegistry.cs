using System;
using System.Collections.Generic;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Registry.Effects;

// ReSharper disable UnusedMemberInSuper.Global

namespace FullPotential.Api.Registry
{
    public interface ITypeRegistry
    {
        IEnumerable<T> GetRegisteredTypes<T>() where T : IRegisterable;

        T GetRegisteredByTypeName<T>(string typeName) where T : IRegisterable;

        IRegisterable GetRegisteredForItem(ItemBase item);

        IEffect GetEffect(Guid typeId);

        IEffect GetEffect(Type type);

        void LoadAddessable(string address, Action<UnityEngine.GameObject> action);
    }
}
