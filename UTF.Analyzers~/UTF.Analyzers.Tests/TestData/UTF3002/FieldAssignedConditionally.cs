using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF3002
{
    public class FieldAssignedConditionally
    {
        private object _target; // CS8618

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return null;
            if (System.Environment.TickCount > 0)
            {
                _target = new object();
            }
        }

        [Test]
        public void Test()
        {
            Assert.That(_target.GetHashCode(), Is.EqualTo(_target.GetHashCode()));
        }
    }
}
