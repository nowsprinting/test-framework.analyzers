using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF2005
{
    public class AfterInLocal
    {
        private bool _flag;

        [Test]
        public void Test()
        {
            var constraint = Is.EqualTo(true).After(1000); // UTF2005
            Assert.That(() => _flag, constraint);
        }
    }
}
