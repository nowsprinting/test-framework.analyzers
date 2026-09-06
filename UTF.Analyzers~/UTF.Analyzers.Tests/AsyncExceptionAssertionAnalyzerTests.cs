using System.Threading.Tasks;
using Xunit;
using Verifier = UTF.Analyzers.Tests.TestDataVerifier<UTF.Analyzers.AsyncExceptionAssertionAnalyzer>;

namespace UTF.Analyzers.Tests
{
    public class AsyncExceptionAssertionAnalyzerTests
    {
        [Theory]
        [InlineData("ThrowsAsyncGeneric", 12, 13, "Assert.ThrowsAsync")]
        [InlineData("ThrowsAsyncType", 12, 13, "Assert.ThrowsAsync")]
        [InlineData("CatchAsyncLambda", 12, 13, "Assert.CatchAsync")]
        [InlineData("CatchAsyncGeneric", 12, 13, "Assert.CatchAsync")]
        [InlineData("DoesNotThrowAsyncMethodGroup", 11, 13, "Assert.DoesNotThrowAsync")]
        [InlineData("UsingStaticAssert", 12, 13, "Assert.DoesNotThrowAsync")]
        public async Task AsyncExceptionAssertion_ReportsAtInvocationWithClassAndMethodName(string fixture, int line,
            int column, string methodName)
        {
            await Verifier.VerifyAsync($"UTF2001/{fixture}.cs",
                Verifier.Diagnostic().WithLocation(line, column).WithArguments(methodName));
        }

        [Fact]
        public async Task MultipleInOneMethod_ReportsEachInvocation()
        {
            await Verifier.VerifyAsync("UTF2001/MultipleInOneMethod.cs",
                Verifier.Diagnostic().WithLocation(12, 21).WithArguments("Assert.ThrowsAsync"),
                Verifier.Diagnostic().WithLocation(13, 13).WithArguments("Assert.DoesNotThrowAsync"));
        }

        [Theory]
        [InlineData("SyncThrows")]
        [InlineData("UserWrapperThrowsAsync")]
        [InlineData("TryCatchGood")]
        public async Task NotAsyncAssertOnNUnitAssert_NoDiagnostic(string fixture)
        {
            await Verifier.VerifyAsync($"UTF2001/{fixture}.cs");
        }
    }
}
