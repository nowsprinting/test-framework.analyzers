using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools.Constraints;
using Is = UnityEngine.TestTools.Constraints.Is;

namespace UTF.Analyzers.Tests.TestData.UTF2004
{
    public class AssumeValueLambda
    {
        private readonly Dictionary<int, int> _dict = new Dictionary<int, int> { { 1, 1 } };

        [Test]
        public void Test()
        {
            Assume.That(() => _dict.Remove(1), Is.Not.AllocatingGCMemory()); // UTF2004
        }
    }
}
