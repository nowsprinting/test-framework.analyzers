using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF2005
{
    public class AfterSingle
    {
        private bool _flag;

        [Test]
        public void Test()
        {
            Assert.That(() => _flag, Is.EqualTo(true).After(1000)); // UTF2005
        }
    }
}
