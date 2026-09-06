using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.TestTools.Constraints;
using Is = UnityEngine.TestTools.Constraints.Is;

namespace UTF.Analyzers.Tests.TestData.UTF2004
{
    public class GoodShapes
    {
        private readonly Dictionary<int, int> _dict = new Dictionary<int, int> { { 1, 1 } };

        [Test]
        public void Test()
        {
            Assert.That(() => { _dict.Remove(1); }, Is.Not.AllocatingGCMemory());
            Assert.That(RemoveOne, Is.Not.AllocatingGCMemory());
            TestDelegate code = () => _dict.Remove(1);
            Assert.That(code, Is.Not.AllocatingGCMemory());
        }

        private void RemoveOne()
        {
            _dict.Remove(1);
        }
    }
}
