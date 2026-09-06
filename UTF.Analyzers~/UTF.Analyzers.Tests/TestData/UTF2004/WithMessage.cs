using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools.Constraints;
using Is = UnityEngine.TestTools.Constraints.Is;

namespace UTF.Analyzers.Tests.TestData.UTF2004
{
    public class WithMessage
    {
        private readonly Dictionary<int, int> _dict = new Dictionary<int, int> { { 1, 1 } };

        [Test]
        public void Test()
        {
            Assert.That(() => _dict.Remove(1), Is.Not.AllocatingGCMemory(), "message"); // UTF2004
        }
    }
}
