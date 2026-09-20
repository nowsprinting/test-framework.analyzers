using System.Threading.Tasks;
using Xunit;
using Verifier = UTF.Analyzers.Tests.TestDataVerifier<UTF.Analyzers.UnboundedWaitWithoutTimeoutAnalyzer>;

namespace UTF.Analyzers.Tests
{
    public class UnboundedWaitWithoutTimeoutAnalyzerTests
    {
        [Theory]
        [InlineData("WhileLoopInCoroutine", 16, 13, "while")]
        [InlineData("DoLoopInAsync", 15, 13, "do")]
        [InlineData("WaitUntilInCoroutine", 16, 26, "WaitUntil")]
        [InlineData("WaitWhileInCoroutine", 16, 26, "WaitWhile")]
        [InlineData("UniTaskWaitUntilInAsync", 15, 19, "UniTask.WaitUntil")]
        [InlineData("UniTaskWaitWhileWithState", 15, 19, "UniTask.WaitWhile")]
        [InlineData("UniTaskWaitUntilValueChanged", 15, 19, "UniTask.WaitUntilValueChanged")]
        [InlineData("UniTaskWaitUntilCanceled", 15, 19, "UniTask.WaitUntilCanceled")]
        [InlineData("LoopInHelper", 16, 26, "WaitForFlag")]
        [InlineData("LoopInNestedHelper", 15, 19, "WaitForFlagAsync")]
        [InlineData("WaitUntilInHelper", 16, 26, "WaitForFlag")]
        [InlineData("LoopInLambda", 18, 17, "while")]
        [InlineData("LoopInLocalFunction", 19, 17, "while")]
        [InlineData("TimeoutOnBaseClass", 21, 13, "while")]
        public async Task UnboundedWaitWithoutTimeout_ReportsAtWait(string fixture, int line, int column, string wait)
        {
            var expected = Verifier.Diagnostic().WithLocation(line, column).WithArguments(wait);
            await Verifier.VerifyAsync($"UTF4001/{fixture}.cs", expected);
        }

        [Fact]
        public async Task MultipleWaits_ReportsEach()
        {
            await Verifier.VerifyAsync("UTF4001/MultipleWaits.cs",
                Verifier.Diagnostic().WithLocation(16, 13).WithArguments("while"),
                Verifier.Diagnostic().WithLocation(21, 26).WithArguments("WaitUntil"));
        }

        [Theory]
        [InlineData("MethodTimeout")]
        [InlineData("ClassTimeout")]
        [InlineData("AssemblyTimeout")]
        [InlineData("BoundedAwaits")]
        [InlineData("BoundedYields")]
        [InlineData("SynchronousLoop")]
        [InlineData("AwaitOnlyInsideLambdaInLoop")]
        [InlineData("LoopInUnitySetUp")]
        [InlineData("HelperBeyondDepthLimit")]
        [InlineData("HelperWithBoundedWait")]
        public async Task TimeoutPresentOrBoundedWait_NoDiagnostic(string fixture)
        {
            await Verifier.VerifyAsync($"UTF4001/{fixture}.cs");
        }
    }
}
