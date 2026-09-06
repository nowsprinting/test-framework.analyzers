using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace UTF.Analyzers.Tests.TestData.UTF2005
{
    public class AfterOnCustomConstraint
    {
        private bool _flag;

        [Test]
        public void Test()
        {
            Assert.That(() => _flag, new CustomConstraint().After(1000)); // UTF2005
        }
    }

    public class CustomConstraint : Constraint
    {
    }
}
