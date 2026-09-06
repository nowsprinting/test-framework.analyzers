using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools.Constraints;
using Is = UnityEngine.TestTools.Constraints.Is;

namespace UTF.Analyzers.Tests.TestData.UTF2004
{
    public class AsyncLambdaOwnedByUTF2003
    {
        private readonly Dictionary<int, int> _dict = new Dictionary<int, int> { { 1, 1 } };

        [Test]
        public void Test()
        {
            Assert.That(async () => { await Task.Yield(); _dict.Remove(1); }, Is.Not.AllocatingGCMemory());
            // Reported by UTF2003: the async delegate is owned by the constraint model rule.
        }
    }
}
