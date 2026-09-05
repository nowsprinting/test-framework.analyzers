using System.Threading.Tasks;
using Xunit;
using Verifier = UTF.Analyzers.Tests.TestDataVerifier<UTF.Analyzers.GenericTaskTestMethodAnalyzer>;

namespace UTF.Analyzers.Tests
{
    public class GenericTaskTestMethodAnalyzerTests
    {
        [Theory]
        [InlineData("TestCaseWithExpectedResultOnGenericTask", 9, 22)]
        [InlineData("TestCaseSourceOnGenericTask", 11, 22)]
        [InlineData("TestOnGenericTask", 9, 22)]
        [InlineData("TestOnNonAsyncGenericTask", 9, 16)]
        [InlineData("MultipleTestCasesOnGenericTask", 10, 22)]
        public async Task GenericTaskTestMethod_ReportsOnceAtReturnType(string fixture, int line, int column)
        {
            await Verifier.VerifyAsync($"UTF1002/{fixture}.cs", Verifier.Diagnostic().WithLocation(line, column));
        }

        [Theory]
        [InlineData("TestCaseOnTask")]
        [InlineData("TestCaseWithExpectedResultOnSync")]
        [InlineData("TestOnGenericValueTask")]
        [InlineData("GenericTaskWithoutTestAttribute")]
        public async Task NotGenericTaskOrNotTestMethod_NoDiagnostic(string fixture)
        {
            await Verifier.VerifyAsync($"UTF1002/{fixture}.cs");
        }
    }
}
