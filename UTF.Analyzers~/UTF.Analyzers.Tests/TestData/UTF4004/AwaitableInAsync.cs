using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;

namespace UTF.Analyzers.Tests.TestData.UTF4004
{

    public class AwaitableInAsync
    {
        [Test]
        public async Task Test()
        {
            await Awaitable.WaitForSecondsAsync(1f);   // UTF4004
        }
    }
}
