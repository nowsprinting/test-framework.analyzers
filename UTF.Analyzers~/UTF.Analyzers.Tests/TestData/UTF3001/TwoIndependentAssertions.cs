using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF3001
{
    public class TwoIndependentAssertions
    {
        [Test]
        public void Test()
        {
            var x = 1;
            var y = 2;
            Assert.That(x, Is.EqualTo(1)); // NUnit2045
            Assert.That(y, Is.EqualTo(2)); // NUnit2045
        }
    }
}
