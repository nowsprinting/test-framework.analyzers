using System.Threading.Tasks;
using Xunit;
using Verifier = UTF.Analyzers.Tests.TestDataVerifier<UTF.Analyzers.UnboundedWaitInSetUpTearDownAnalyzer>;

namespace UTF.Analyzers.Tests
{
    public class UnboundedWaitInSetUpTearDownAnalyzerTests
    {
        [Theory]
        [InlineData("LoopInUnitySetUp", 15, 13, "while", "UnitySetUpAttribute")]
        [InlineData("WaitUntilInUnityTearDown", 16, 26, "WaitUntil", "UnityTearDownAttribute")]
        [InlineData("WaitWhileInUnityOneTimeTearDown", 16, 26, "WaitWhile", "UnityOneTimeTearDownAttribute")]
        [InlineData("LoopInHelperFromUnityOneTimeSetUp", 15, 26, "WaitForFlag", "UnityOneTimeSetUpAttribute")]
        [InlineData("UniTaskWaitUntilInAsyncSetUp", 15, 19, "UniTask.WaitUntil", "SetUpAttribute")]
        [InlineData("DoLoopInAsyncTearDown", 15, 13, "do", "TearDownAttribute")]
        [InlineData("UniTaskWaitUntilWithCancellationToken", 17, 19, "UniTask.WaitUntil", "SetUpAttribute")]
        [InlineData("ClassTimeout", 16, 13, "while", "UnitySetUpAttribute")]
        [InlineData("OverrideOfUnitySetUp", 20, 13, "while", "UnitySetUpAttribute")]
        public async Task UnboundedWaitInSetUpTearDown_ReportsAtWait(string fixture, int line, int column,
            string wait, string attribute)
        {
            var expected = Verifier.Diagnostic().WithLocation(line, column).WithArguments(wait, attribute);
            await Verifier.VerifyAsync($"UTF4002/{fixture}.cs", expected);
        }

        [Fact]
        public async Task MultipleWaits_ReportsEach()
        {
            await Verifier.VerifyAsync("UTF4002/MultipleWaits.cs",
                Verifier.Diagnostic().WithLocation(16, 13).WithArguments("while", "UnitySetUpAttribute"),
                Verifier.Diagnostic().WithLocation(21, 26).WithArguments("WaitUntil", "UnitySetUpAttribute"));
        }

        [Theory]
        [InlineData("BoundedWaitUntilInUnitySetUp")]
        [InlineData("UniTaskTimeoutChain")]
        [InlineData("ForLoopInUnitySetUp")]
        [InlineData("UnitySetUpReturningTask")]
        [InlineData("LoopInTestMethod")]
        public async Task BoundedWaitOrNotRunAsCoroutine_NoDiagnostic(string fixture)
        {
            await Verifier.VerifyAsync($"UTF4002/{fixture}.cs");
        }
    }
}
