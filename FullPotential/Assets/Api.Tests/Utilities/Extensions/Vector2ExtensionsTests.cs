using FullPotential.Api.Unity.Extensions;
using NUnit.Framework;
using UnityEngine;

namespace FullPotential.Api.Tests.Utilities.Extensions
{
    [TestFixture]
    public class Vector2ExtensionsTests
    {
        [Test]
        public void ClockwiseAngleTo_GivenOver180Degrees_GivesExpectedValue()
        {
            var vector = new Vector2(-20, 20);

            var result = Vector2.up.ClockwiseAngleTo(vector.normalized);

            Assert.AreEqual(315, result);
        }
    }
}
