using System.Threading.Tasks;
using Xunit;
using Verifier = UTF.Analyzers.Tests.TestDataVerifier<UTF.Analyzers.BusyWaitAnalyzer>;

namespace UTF.Analyzers.Tests
{
    public class BusyWaitAnalyzerTests
    {
        [Theory]
        [InlineData("EmptyLoopInTest", 13, 13, "while")]
        [InlineData("SleepInLoop", 14, 13, "while")]
        [InlineData("SingleStatementSleepLoop", 14, 13, "while")]
        [InlineData("EmptyStatementLoop", 13, 13, "while")]
        [InlineData("SpinMethodsInLoop", 15, 13, "while")]
        [InlineData("DoLoop", 13, 13, "do")]
        [InlineData("SpinUntilInTest", 14, 13, "SpinWait.SpinUntil")]
        [InlineData("EmptyLoopInCoroutineTest", 14, 13, "while")]
        [InlineData("EmptyLoopInAsyncTest", 14, 13, "while")]
        [InlineData("EmptyLoopInUnitySetUp", 15, 13, "while")]
        [InlineData("EmptyLoopInOneTimeSetUp", 13, 13, "while")]
        [InlineData("EmptyLoopInAsyncVoidTearDown", 14, 13, "while")]
        [InlineData("OverrideOfUnitySetUp", 19, 13, "while")]
        [InlineData("LoopInLambda", 16, 17, "while")]
        [InlineData("LoopInTaskRunLambda", 16, 17, "while")]
        [InlineData("LoopInLocalFunction", 17, 17, "while")]
        [InlineData("LoopInHelper", 19, 13, "while")]
        [InlineData("LoopInAsyncHelper", 19, 13, "while")]
        [InlineData("HelperCalledTwice", 25, 13, "while")]
        [InlineData("LoopInNestedHelper", 24, 13, "while")]
        public async Task BusyWait_ReportsAtWait(string fixture, int line, int column, string wait)
        {
            var expected = Verifier.Diagnostic().WithLocation(line, column).WithArguments(wait);
            await Verifier.VerifyAsync($"UTF4003/{fixture}.cs", expected);
        }

        [Fact]
        public async Task SpinUntilWithTimeout_ReportsEachOverload()
        {
            await Verifier.VerifyAsync("UTF4003/SpinUntilWithTimeout.cs",
                Verifier.Diagnostic().WithLocation(15, 21).WithArguments("SpinWait.SpinUntil"),
                Verifier.Diagnostic().WithLocation(16, 21).WithArguments("SpinWait.SpinUntil"));
        }

        [Fact]
        public async Task MultipleWaits_ReportsEach()
        {
            await Verifier.VerifyAsync("UTF4003/MultipleWaits.cs",
                Verifier.Diagnostic().WithLocation(14, 13).WithArguments("while"),
                Verifier.Diagnostic().WithLocation(18, 13).WithArguments("SpinWait.SpinUntil"));
        }

        [Theory]
        [InlineData("LoopThatDoesWork")]
        [InlineData("YieldInLoop")]
        [InlineData("AwaitInLoop")]
        [InlineData("ForLoop")]
        [InlineData("SingleSleepOutsideLoop")]
        [InlineData("LoopInSynchronousHelper")]
        [InlineData("HelperBeyondDepthLimit")]
        [InlineData("LoopInNonTestMethod")]
        public async Task LoopThatWorksOrOutOfScope_NoDiagnostic(string fixture)
        {
            await Verifier.VerifyAsync($"UTF4003/{fixture}.cs");
        }
    }
}
