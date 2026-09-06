using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace UTF.Analyzers.Tests.TestData.UTF2005
{
    public class NoDelayGood
    {
        private bool _flag;

        [Test]
        public void Test()
        {
            var name = nameof(Constraint.After);
            Assert.That(() => _flag, Is.EqualTo(true), name);
        }
    }
}
