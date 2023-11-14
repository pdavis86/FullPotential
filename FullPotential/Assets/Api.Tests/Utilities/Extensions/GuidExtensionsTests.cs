using System;
using FullPotential.Api.Utilities.Extensions;
using NUnit.Framework;

namespace FullPotential.Api.Tests.Utilities.Extensions
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