using System.Threading.Tasks;
using Xunit;
using Verifier = UTF.Analyzers.Tests.TestDataVerifier<UTF.Analyzers.FixedTimeWaitAnalyzer>;

namespace UTF.Analyzers.Tests
{
    public class FixedTimeWaitAnalyzerTests
    {
        [Theory]
        [InlineData("WaitForSecondsInCoroutine", 13, 26, "WaitForSeconds")]
        [InlineData("WaitForSecondsRealtimeInCoroutine", 13, 26, "WaitForSecondsRealtime")]
        [InlineData("CachedWaitForSeconds", 15, 26, "WaitForSeconds")]
        [InlineData("UniTaskDelayInAsync", 13, 19, "UniTask.Delay")]
        [InlineData("UniTaskWaitForSecondsInAsync", 13, 19, "UniTask.WaitForSeconds")]
        [InlineData("TaskDelayInAsync", 13, 19, "Task.Delay")]
        [InlineData("TaskDelayWithConfigureAwait", 12, 19, "Task.Delay")]
        [InlineData("AwaitableInAsync", 13, 19, "Awaitable.WaitForSecondsAsync")]
        [InlineData("ThreadSleepInTest", 12, 13, "Thread.Sleep")]
        [InlineData("FixedWaitInForLoop", 15, 30, "WaitForSeconds")]
        [InlineData("FixedWaitInUnitySetUp", 13, 26, "WaitForSeconds")]
        [InlineData("FixedWaitInOneTimeTearDown", 12, 13, "Thread.Sleep")]
        [InlineData("FixedWaitInLambda", 15, 23, "Task.Delay")]
        [InlineData("FixedWaitInLambdaInsideLoop", 18, 38, "Thread.Sleep")]
        [InlineData("FixedWaitInLocalFunction", 16, 17, "Thread.Sleep")]
        [InlineData("FixedWaitInHelper", 18, 26, "WaitForSeconds")]
        [InlineData("HelperCalledTwice", 24, 26, "WaitForSeconds")]
        public async Task FixedWait_ReportsAtWait(string fixture, int line, int column, string wait)
        {
            var expected = Verifier.Diagnostic().WithLocation(line, column).WithArguments(wait);
            await Verifier.VerifyAsync($"UTF4004/{fixture}.cs", expected);
        }

        [Fact]
        public async Task MultipleWaits_ReportsEach()
        {
            await Verifier.VerifyAsync("UTF4004/MultipleWaits.cs",
                Verifier.Diagnostic().WithLocation(13, 13).WithArguments("Thread.Sleep"),
                Verifier.Diagnostic().WithLocation(14, 19).WithArguments("Task.Delay"));
        }

        [Theory]
        [InlineData("PollingLoop")]
        [InlineData("SleepInDoLoop")]
        [InlineData("FrameWaits")]
        [InlineData("WaitForSecondsNotYielded")]
        [InlineData("FixedWaitInNonTestMethod")]
        [InlineData("HelperCalledAsStatement")]
        [InlineData("HelperBeyondDepthLimit")]
        public async Task ConditionWaitOrOutOfScope_NoDiagnostic(string fixture)
        {
            await Verifier.VerifyAsync($"UTF4004/{fixture}.cs");
        }
    }
}
