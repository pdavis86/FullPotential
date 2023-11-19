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

        T GetRegisteredByTypeId<T>(string typeId) where T : IRegisterable;

        IRegisterable GetAnyRegisteredBySlotId(string typeId);

        IRegisterable GetRegistryTypeForItem(ItemBase item);

        IEffect GetEffect(string typeId);

        void LoadAddessable<T>(string address, Action<T> action);
    }
}
