using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF2005
{
    public class UserAfterMethod
    {
        private bool _flag;

        [Test]
        public void Test()
        {
            var expected = new Waiter().After(1000);
            Assert.That(() => _flag, Is.EqualTo(expected));
        }
    }

    public class Waiter
    {
        public bool After(int delayInMilliseconds)
        {
            return true;
        }
    }
}
