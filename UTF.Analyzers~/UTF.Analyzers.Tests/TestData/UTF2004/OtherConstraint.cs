using System.Collections.Generic;
using NUnit.Framework;
using Is = UnityEngine.TestTools.Constraints.Is;

namespace UTF.Analyzers.Tests.TestData.UTF2004
{
    public class OtherConstraint
    {
        private readonly Dictionary<int, int> _dict = new Dictionary<int, int> { { 1, 1 } };

        [Test]
        public void Test()
        {
            Assert.That(() => _dict.Remove(1), Is.EqualTo(true));
        }
    }
}
