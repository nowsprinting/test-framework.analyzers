using System.Threading.Tasks;
using Xunit;
using Verifier = UTF.Analyzers.Tests.TestDataVerifier<UTF.Analyzers.PreferAsyncTestMethodAnalyzer>;

namespace UTF.Analyzers.Tests
{
    public class PreferAsyncTestMethodAnalyzerTests
    {
        [Theory]
        [InlineData("YieldNull", 11)]
        [InlineData("YieldWaitUntil", 13)]
        [InlineData("YieldAsyncOperation", 11)]
        [InlineData("YieldCachedInstruction", 13)]
        [InlineData("YieldFixtureHelper", 13)]
        [InlineData("YieldBaseClassHelper", 11)]
        [InlineData("MeasureFramesScope", 12)]
        [InlineData("NoYieldReturn", 11)]
        [InlineData("YieldNullInLoop", 13)]
        public async Task ConvertibleCoroutineTest_ReportsAtMethodIdentifier(string fixture, int line)
        {
            var expected = Verifier.Diagnostic().WithLocation(line, 28).WithArguments("Test");
            await Verifier.VerifyAsync($"UTF4006/{fixture}.cs", expected);
        }

        [Theory]
        [InlineData("YieldCoroutineUnderTest")]
        [InlineData("YieldStartCoroutine")]
        [InlineData("MeasureFramesRun")]
        [InlineData("YieldCustomInstructionUnderTest")]
        [InlineData("YieldEnterPlayMode")]
        [InlineData("NonIteratorHelper")]
        [InlineData("NonIteratorTestMethod")]
        [InlineData("HelperBeyondDepthLimit")]
        [InlineData("HelperOnOtherType")]
        [InlineData("YieldEnumeratorVariable")]
        [InlineData("YieldNullInLambda")]
        [InlineData("AsyncTestMethod")]
        public async Task NotConvertibleOrNotCoroutine_NoDiagnostic(string fixture)
        {
            await Verifier.VerifyAsync($"UTF4006/{fixture}.cs");
        }
    }
}
