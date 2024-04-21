using System;

namespace FullPotential.Api.Registry
{
    public interface IRegisterableType
    {
        /// <summary>
        /// The GUID for this type of registerable
        /// </summary>
        Guid TypeId { get; }
    }
}
