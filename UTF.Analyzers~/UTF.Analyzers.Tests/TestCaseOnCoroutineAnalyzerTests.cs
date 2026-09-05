using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Verifier = UTF.Analyzers.Tests.TestDataVerifier<UTF.Analyzers.TestCaseOnCoroutineAnalyzer>;

namespace UTF.Analyzers.Tests
{
    public class TestCaseOnCoroutineAnalyzerTests
    {
        [Theory]
        [InlineData("TestCaseOnCoroutine", "TestCaseAttribute", new[] { 8, 9 })]
        [InlineData("TestCaseSourceOnCoroutine", "TestCaseSourceAttribute", new[] { 10 })]
        [InlineData("TestCaseWithExpectedResultOnCoroutine", "TestCaseAttribute", new[] { 8 })]
        [InlineData("UnityTestWithTestCaseOnCoroutine", "TestCaseAttribute", new[] { 10 })]
        public async Task IEnumeratorMethodWithMethodLevelParameterizedAttribute_ReportsAtEachAttributeWithItsTypeName(string fixture, string attributeName, int[] lines)
        {
            var expected = lines.Select(line => Verifier.Diagnostic().WithLocation(line, 10).WithArguments(attributeName)).ToArray();
            await Verifier.VerifyAsync($"UTF1001/{fixture}.cs", expected);
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
