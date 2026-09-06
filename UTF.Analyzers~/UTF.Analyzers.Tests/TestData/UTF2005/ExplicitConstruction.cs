using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace UTF.Analyzers.Tests.TestData.UTF2005
{
    public class ExplicitConstruction
    {
        private bool _flag;

        [Test]
        public void Test()
        {
            Assert.That(() => _flag, new DelayedConstraint(Is.EqualTo(true), 1000)); // UTF2005
        }
    }
}
