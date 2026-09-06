using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF3002
{
    public class FieldAssignedInSetUp
    {
        private object _target; // CS8618

        [SetUp]
        public void SetUp()
        {
            _target = new object();
        }

        [Test]
        public void Test()
        {
            Assert.That(_target.GetHashCode(), Is.EqualTo(_target.GetHashCode()));
        }
    }
}
