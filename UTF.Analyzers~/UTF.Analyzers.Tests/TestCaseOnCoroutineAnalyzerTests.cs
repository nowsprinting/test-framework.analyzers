using System.Threading.Tasks;
using Xunit;
using Verifier = UTF.Analyzers.Tests.TestDataVerifier<UTF.Analyzers.TestCaseOnCoroutineAnalyzer>;

namespace UTF.Analyzers.Tests
{
    public class TestCaseOnCoroutineAnalyzerTests
    {
        [Theory]
        [InlineData("TestCaseOnCoroutine", 10)]
        [InlineData("TestCaseSourceOnCoroutine", 11)]
        [InlineData("TestCaseWithExpectedResultOnCoroutine", 9)]
        [InlineData("UnityTestWithTestCaseOnCoroutine", 11)]
        public async Task IEnumeratorMethodWithMethodLevelParameterizedAttribute_ReportsOnce(string fixture, int line)
        {
            await Verifier.VerifyAsync($"UTF1001/{fixture}.cs", Verifier.Diagnostic().WithLocation(line, 28));
        }

        [Theory]
        [InlineData("TestCaseOnAsyncTask")]
        [InlineData("TestCaseOnVoid")]
        [InlineData("UnityTestWithValueSource")]
        [InlineData("TestCaseOnGenericEnumerator")]
        public async Task NotCoroutineOrParameterLevelAttribute_NoDiagnostic(string fixture)
        {
            await Verifier.VerifyAsync($"UTF1001/{fixture}.cs");
        }
    }
}
