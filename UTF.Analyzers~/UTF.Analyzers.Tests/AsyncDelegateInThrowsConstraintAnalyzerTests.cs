using System.Threading.Tasks;
using Xunit;
using Verifier = UTF.Analyzers.Tests.TestDataVerifier<UTF.Analyzers.AsyncDelegateInThrowsConstraintAnalyzer>;

namespace UTF.Analyzers.Tests
{
    public class AsyncDelegateInThrowsConstraintAnalyzerTests
    {
        [Theory]
        [InlineData("ThrowsTypeOfAsyncLambda", 12, 25, "Throws.TypeOf")]
        [InlineData("ThrowsInstanceOfTaskLambda", 12, 25, "Throws.InstanceOf")]
        [InlineData("ThrowsNothingMethodGroup", 12, 25, "Throws.Nothing")]
        [InlineData("ThrowsExceptionPropertyRoot", 12, 25, "Throws.Exception")]
        [InlineData("ThrowsTypeOfGenericTaskLambdaWithMessage", 12, 25, "Throws.TypeOf")]
        [InlineData("UsingStaticThrows", 13, 25, "Throws.TypeOf")]
        [InlineData("ThrowsTypeOfValueTaskAsyncLambda", 12, 25, "Throws.TypeOf")]
        [InlineData("ThrowsTypeOfUniTaskLambda", 12, 25, "Throws.TypeOf")]
        [InlineData("ThrowsNothingAwaitableMethodGroup", 12, 25, "Throws.Nothing")]
        [InlineData("NewThrowsConstraint", 13, 25, "ThrowsConstraint")]
        [InlineData("NewThrowsNothingConstraint", 12, 25, "ThrowsNothingConstraint")]
        [InlineData("AssertThrowsAsyncLambda", 12, 54, "Assert.Throws")]
        [InlineData("AssertThrowsTypeAsyncLambda", 12, 62, "Assert.Throws")]
        [InlineData("AssertCatchAsyncLambda", 12, 26, "Assert.Catch")]
        [InlineData("AssertDoesNotThrowAsyncVoidMethodGroup", 12, 33, "Assert.DoesNotThrow")]
        public async Task AsyncDelegate_ReportsAtDelegateArgumentWithReceivingApi(string fixture, int line, int column,
            string api)
        {
            await Verifier.VerifyAsync($"UTF2002/{fixture}.cs",
                Verifier.Diagnostic().WithLocation(line, column).WithArguments(api));
        }

        [Fact]
        public async Task MultipleInOneMethod_ReportsEachArgument()
        {
            await Verifier.VerifyAsync("UTF2002/MultipleInOneMethod.cs",
                Verifier.Diagnostic().WithLocation(12, 25).WithArguments("Throws.Nothing"),
                Verifier.Diagnostic().WithLocation(13, 54).WithArguments("Assert.Throws"));
        }

        [Theory]
        [InlineData("SyncDelegates")]
        [InlineData("TaskResultInsteadOfDelegate")]
        [InlineData("NonThrowsConstraintOnAsyncLambda")]
        [InlineData("TryCatchGood")]
        public async Task NotAsyncDelegateInReportedApi_NoDiagnostic(string fixture)
        {
            await Verifier.VerifyAsync($"UTF2002/{fixture}.cs");
        }
    }
}
