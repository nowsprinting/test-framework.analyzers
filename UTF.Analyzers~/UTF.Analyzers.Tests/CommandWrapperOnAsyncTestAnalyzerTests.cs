using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Verifier = UTF.Analyzers.Tests.TestDataVerifier<UTF.Analyzers.CommandWrapperOnAsyncTestAnalyzer>;

namespace UTF.Analyzers.Tests
{
    public class CommandWrapperOnAsyncTestAnalyzerTests
    {
        [Theory]
        [InlineData("WrapTestMethodOnAsyncTask", 11, "WrapTestMethodOnAsyncTaskWrapperAttribute")]
        [InlineData("WrapTestMethodOnCoroutine", 12, "WrapTestMethodOnCoroutineWrapperAttribute")]
        [InlineData("WrapSetUpTearDownOnAsyncTask", 11, "WrapSetUpTearDownOnAsyncTaskWrapperAttribute")]
        [InlineData("WrapSetUpTearDownOnCoroutine", 12, "WrapSetUpTearDownOnCoroutineWrapperAttribute")]
        [InlineData("WrapperOnTestCaseAsyncTask", 11, "WrapperOnTestCaseAsyncTaskWrapperAttribute")]
        [InlineData("DerivedWrapperOnAsyncTask", 11, "DerivedWrapperOnAsyncTaskDerivedAttribute")]
        [InlineData("DerivedRepeatOnAsyncTask", 9, "DerivedRepeatOnAsyncTaskRepeatAttribute")]
        public async Task WrapperAttributeOnAsyncOrCoroutineTest_ReportsAtAttributeWithItsTypeName(string fixture, int line, string attributeName)
        {
            var expected = Verifier.Diagnostic().WithLocation(line, 10).WithArguments(attributeName);
            await Verifier.VerifyAsync($"UTF1005/{fixture}.cs", expected);
        }

        [Fact]
        public async Task MultipleWrapperAttributes_ReportsEachAttribute()
        {
            var expected = new[]
            {
                Verifier.Diagnostic().WithLocation(11, 10).WithArguments("MultipleWrappersOnAsyncTaskFirstAttribute"),
                Verifier.Diagnostic().WithLocation(12, 10).WithArguments("MultipleWrappersOnAsyncTaskSecondAttribute"),
            };
            await Verifier.VerifyAsync("UTF1005/MultipleWrappersOnAsyncTask.cs", expected);
        }

        [Theory]
        [InlineData("WrapperOnSync")]
        [InlineData("ExemptAttributes")]
        [InlineData("WrapperOnGenericTask")]
        [InlineData("WrapperOnSetUp")]
        [InlineData("NonWrapperOnAsyncTask")]
        public async Task SyncTestOrExemptOrNonTestMethod_NoDiagnostic(string fixture)
        {
            await Verifier.VerifyAsync($"UTF1005/{fixture}.cs");
        }
    }
}
