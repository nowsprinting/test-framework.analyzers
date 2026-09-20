using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF1007
{
    public class SetUpOnAsyncTask
    {
        [SetUp]
        public async Task SetUp()
        {
            await UniTask.NextFrame();
        }
    }
}
