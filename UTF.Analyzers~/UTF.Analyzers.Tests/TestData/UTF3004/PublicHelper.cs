using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF3004
{
    public class PublicHelper
    {
        public void Helper() // NUnit1028
        {
        }

        [Test]
        public void MyTestMethod()
        {
        }
    }
}
