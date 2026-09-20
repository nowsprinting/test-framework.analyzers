using Cysharp.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF1007
{
    public class SetUpOnNonAsyncUniTask
    {
        [SetUp]
        public UniTask SetUp() // UTF1007
        {
            return UniTask.NextFrame();
        }
    }
}
