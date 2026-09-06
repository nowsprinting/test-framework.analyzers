using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Cysharp.Threading.Tasks;

namespace UTF.Analyzers.Tests.TestData.UTF2003
{
    public class UniTaskLambda
    {

        [Test]
        public void Test()
        {
            Assert.That(() => GetUniTask(), Is.EqualTo(1)); // UTF2003
        }

        private static UniTask GetUniTask()
        {
            throw new NotImplementedException();
        }

        private static async Task<int> GetAsync()
        {
            await Task.Yield();
            return 1;
        }
    }
}
