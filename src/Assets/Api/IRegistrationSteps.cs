using System;
using System.Collections.Generic;

// ReSharper disable UnusedMemberInSuper.Global

namespace Assets.ApiScripts
{
    public interface IRegistrationSteps
    {
        IEnumerable<Type> GetRegisterables();
    }
}
