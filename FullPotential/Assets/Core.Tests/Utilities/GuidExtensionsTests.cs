using System;
using FullPotential.Core.Utilities.Extensions;
using NUnit.Framework;

namespace FullPotential.Core.Tests.Utilities
{
    public class GuidExtensionsTests
    {
        [Test]
        public void ToMinimisedString_Base64String()
        {
            var data = Guid.NewGuid();

            var result = data.ToMinimisedString();

            Assert.AreEqual(Convert.ToBase64String(data.ToByteArray()), result);
        }
    }
}