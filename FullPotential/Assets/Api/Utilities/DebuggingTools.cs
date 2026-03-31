using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace FullPotential.Api.Utilities
{
    public static class DebuggingTools
    {
        public static T RunSynchronously<T>(this Awaitable<T> awaitable)
        {
            return Task.Run(async () => await awaitable).GetAwaiter().GetResult();
        }
    }
}
