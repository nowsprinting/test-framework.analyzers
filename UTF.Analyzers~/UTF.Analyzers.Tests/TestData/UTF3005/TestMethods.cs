using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF3005
{
    public class TestMethods
    {
        [Test]
        public void Add_TwoPositiveNumbers_ReturnsSum() // CA1707
        {
        }

        [UnityTest]
        public IEnumerator LoadScene_SceneExists_IsLoadedAfterOneFrame() // CA1707
        {
            yield return null;
        }

        [TestCase(1)]
        [TestCase(2)]
        public void IsPositive_PositiveNumber_ReturnsTrue(int value) // CA1707
        {
        }

        [TestCaseSource(nameof(Cases))]
        public void IsPositive_FromSource_ReturnsTrue(int value) // CA1707
        {
        }

        private static readonly int[] Cases = { 1, 2 };
    }
}
