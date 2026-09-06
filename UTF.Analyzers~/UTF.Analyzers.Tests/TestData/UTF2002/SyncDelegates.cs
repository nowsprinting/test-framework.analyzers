using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF2002
{
    public class SyncDelegates
    {
        [Test]
        public void Test()
        {
            Assert.That(() => Foo(), Throws.TypeOf<InvalidOperationException>());
            Assert.That(Foo, Throws.Exception.TypeOf<InvalidOperationException>());
            Assert.Throws<InvalidOperationException>(() => Foo());
            Assert.Catch(Foo);
            Assert.DoesNotThrow(() => { });
        }

        private static void Foo()
        {
            throw new InvalidOperationException("boom");
        }
    }
}
