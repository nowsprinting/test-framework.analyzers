using System.Threading.Tasks;
using Xunit;
using Verifier = UTF.Analyzers.Tests.TestDataVerifier<UTF.Analyzers.AsyncDelegateInConstraintModelAnalyzer>;

namespace UTF.Analyzers.Tests
{
    public class AsyncDelegateInConstraintModelAnalyzerTests
    {
        [Theory]
        [InlineData("IsEqualToAsyncLambda", 13, "Assert.That")]
        [InlineData("IsEqualToTaskLambda", 13, "Assert.That")]
        [InlineData("IsEqualToMethodGroup", 13, "Assert.That")]
        [InlineData("IsEqualToWithMessage", 13, "Assert.That")]
        [InlineData("NewEqualConstraint", 14, "Assert.That")]
        [InlineData("DelegateVariable", 15, "Assert.That")]
        [InlineData("ValueTaskAsyncLambda", 13, "Assert.That")]
        [InlineData("UniTaskLambda", 14, "Assert.That")]
        [InlineData("ThrowsConstraintInVariable", 14, "Assert.That")]
        [InlineData("ThrowsConstraintInField", 15, "Assert.That")]
        [InlineData("ThrowsConstraintFromParameter", 19, "Assert.That")]
        [InlineData("AssumeIsEqualToAsyncLambda", 13, "Assume.That")]
        [InlineData("AssumeThrowsTypeOf", 13, "Assume.That")]
        public async Task AsyncDelegate_ReportsAtDelegateArgumentWithReceivingApi(string fixture, int line, string api)
        {
            await Verifier.VerifyAsync($"UTF2003/{fixture}.cs",
                Verifier.Diagnostic().WithLocation(line, 25).WithArguments(api));
        }

        [Fact]
        public async Task MultipleInOneMethod_ReportsEachArgument()
        {
            await Verifier.VerifyAsync("UTF2003/MultipleInOneMethod.cs",
                Verifier.Diagnostic().WithLocation(13, 25).WithArguments("Assume.That"),
                Verifier.Diagnostic().WithLocation(14, 25).WithArguments("Assert.That"));
        }

        [Theory]
        [InlineData("SyncDelegates")]
        [InlineData("TaskResultInsteadOfDelegate")]
        [InlineData("ThrowsConstraintOnAsyncLambda")]
        [InlineData("AwaitThenAssertGood")]
        public async Task NotAsyncDelegateOrReportedByUTF2002_NoDiagnostic(string fixture)
        {
            await Verifier.VerifyAsync($"UTF2003/{fixture}.cs");
        }
    }
}
