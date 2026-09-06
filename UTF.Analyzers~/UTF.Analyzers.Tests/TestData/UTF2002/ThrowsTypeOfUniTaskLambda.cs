using System;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF2002
{
    public class ThrowsTypeOfUniTaskLambda
    {
        [Test]
        public void Test()
        {
            Assert.That(() => FooAsync(), Throws.TypeOf<InvalidOperationException>()); // UTF2002
        }

        private static UniTask FooAsync()
        {
            throw new InvalidOperationException("boom");
        }
    }
}
