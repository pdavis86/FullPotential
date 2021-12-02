using System;
using System.Collections.Generic;

// ReSharper disable UnusedMemberInSuper.Global

namespace FullPotential.Api
{
    public interface IRegistrationSteps
    {
        IEnumerable<Type> GetRegisterables();
    }
}
