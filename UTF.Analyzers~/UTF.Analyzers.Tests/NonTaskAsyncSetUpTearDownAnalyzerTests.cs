using System.Threading.Tasks;
using Xunit;
using Verifier = UTF.Analyzers.Tests.TestDataVerifier<UTF.Analyzers.NonTaskAsyncSetUpTearDownAnalyzer>;

namespace UTF.Analyzers.Tests
{
    public class NonTaskAsyncSetUpTearDownAnalyzerTests
    {
        [Theory]
        [InlineData("SetUpOnAsyncVoid", 9, 22, "void")]
        [InlineData("TearDownOnAsyncValueTask", 9, 22, "ValueTask")]
        [InlineData("SetUpOnNonAsyncUniTask", 9, 16, "UniTask")]
        [InlineData("SetUpOnNonAsyncGenericTask", 9, 16, "Task<int>")]
        [InlineData("TearDownOnNonAsyncAwaitable", 9, 16, "Awaitable")]
        [InlineData("BothAttributesOnAsyncVoid", 10, 22, "void")]
        public async Task NonTaskAsyncSetUpTearDown_ReportsOnceAtReturnTypeWithItsName(string fixture, int line,
            int column, string returnType)
        {
            await Verifier.VerifyAsync($"UTF1007/{fixture}.cs",
                Verifier.Diagnostic().WithLocation(line, column).WithArguments(returnType));
        }

        [Theory]
        [InlineData("SetUpOnAsyncTask")]
        [InlineData("TearDownOnSyncVoid")]
        [InlineData("SetUpOnSyncInt")]
        [InlineData("OneTimeSetUpOnAsyncVoid")]
        [InlineData("UnitySetUpOnAsyncTask")]
        [InlineData("AsyncVoidWithoutAttribute")]
        public async Task TaskOrSyncOrNotSetUpTearDown_NoDiagnostic(string fixture)
        {
            await Verifier.VerifyAsync($"UTF1007/{fixture}.cs");
        }
    }
}
