using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF3002
{
    public class MembersAssignedAndUnassignedInUnitySetUp
    {
        private object _first; // CS8618
        private object _second; // CS8618
        private object _unassigned; // CS8618

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            _first = new object();
            Initialize();
            if (_first.GetHashCode() > 0)
            {
                _unassigned = new object();
            }

            yield return null;
        }

        private void Initialize()
        {
            _second = new object();
        }

        [Test]
        public void Test()
        {
            Assert.That(_first.GetHashCode(), Is.EqualTo(_first.GetHashCode()));
            Assert.That(_second.GetHashCode(), Is.EqualTo(_second.GetHashCode()));
            Assert.That(_unassigned.GetHashCode(), Is.EqualTo(_unassigned.GetHashCode()));
        }
    }
}
