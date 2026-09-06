using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF3002
{
    public abstract class SetUpBase
    {
        [UnitySetUp]
        public abstract IEnumerator SetUp();
    }

    public class FieldAssignedInOverriddenUnitySetUp : SetUpBase
    {
        private object _target; // CS8618

        public override IEnumerator SetUp()
        {
            _target = new object();
            yield return null;
        }

        [Test]
        public void Test()
        {
            Assert.That(_target.GetHashCode(), Is.EqualTo(_target.GetHashCode()));
        }
    }
}
