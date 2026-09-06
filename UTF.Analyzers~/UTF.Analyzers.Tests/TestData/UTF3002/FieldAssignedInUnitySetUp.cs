using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF3002
{
    public class FieldAssignedInUnitySetUp
    {
        private object _target; // CS8618

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return null;
            _target = new object();
        }

        [Test]
        public void Test()
        {
            Assert.That(_target.GetHashCode(), Is.EqualTo(_target.GetHashCode()));
        }
    }
}
