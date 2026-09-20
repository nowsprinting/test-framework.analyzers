using Cysharp.Threading.Tasks;

namespace UTF.Analyzers.Tests.TestData.UTF1006
{
    public class UniTaskWithoutTestAttribute
    {
        public UniTask ReturnsUniTask()
        {
            return UniTask.NextFrame();
        }
    }
}
