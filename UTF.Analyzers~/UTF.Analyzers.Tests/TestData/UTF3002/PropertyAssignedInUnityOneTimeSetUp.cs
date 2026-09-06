using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF3002
{
    public class PropertyAssignedInUnityOneTimeSetUp
    {
        private object Target { get; set; } // CS8618

        [UnityOneTimeSetUp]
        public IEnumerator OneTimeSetUp()
        {
            this.Target = new object();
            yield return null;
        }

        [Test]
        public void Test()
        {
            Assert.That(Target.GetHashCode(), Is.EqualTo(Target.GetHashCode()));
        }
    }
}
