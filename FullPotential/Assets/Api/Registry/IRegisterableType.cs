using System;

namespace FullPotential.Api.Registry
{
    public interface IRegisterableType
    {
        // todo: make IRegisterableType.TypeId a string for simplicity

        /// <summary>
        /// The GUID for this type of registerable
        /// </summary>
        Guid TypeId { get; }
    }
}
