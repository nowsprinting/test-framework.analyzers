using System.Threading.Tasks;
using Xunit;
using Verifier = UTF.Analyzers.Tests.TestDataVerifier<UTF.Analyzers.NonTaskAsyncTestMethodAnalyzer>;

namespace UTF.Analyzers.Tests
{
    public class NonTaskAsyncTestMethodAnalyzerTests
    {
        [Theory]
        [InlineData("TestOnAsyncValueTask", 9, 22, "ValueTask")]
        [InlineData("TestOnNonAsyncUniTask", 9, 16, "UniTask")]
        [InlineData("TestCaseOnUniTask", 9, 16, "UniTask")]
        [InlineData("TestCaseSourceOnUniTask", 11, 16, "UniTask")]
        [InlineData("TestOnNonAsyncAwaitable", 9, 16, "Awaitable")]
        [InlineData("MultipleTestCasesOnUniTask", 10, 16, "UniTask")]
        public async Task NonTaskAsyncTestMethod_ReportsOnceAtReturnTypeWithItsName(string fixture, int line,
            int column, string returnType)
        {
            await Verifier.VerifyAsync($"UTF1006/{fixture}.cs",
                Verifier.Diagnostic().WithLocation(line, column).WithArguments(returnType));
        }

        [Theory]
        [InlineData("TestOnAsyncTask")]
        [InlineData("TestCaseWithExpectedResultOnSync")]
        [InlineData("TestOnAsyncVoid")]
        [InlineData("TestOnGenericTask")]
        [InlineData("UniTaskWithoutTestAttribute")]
        [InlineData("UnityTestOnCoroutine")]
        public async Task TaskOrVoidOrNotTestMethod_NoDiagnostic(string fixture)
        {
            await Verifier.VerifyAsync($"UTF1006/{fixture}.cs");
        }
    }
}
