using System.Threading.Tasks;
using Xunit;
using Verifier = UTF.Analyzers.Tests.TestDataVerifier<UTF.Analyzers.AsyncOneTimeSetUpTearDownAnalyzer>;

namespace UTF.Analyzers.Tests
{
    public class AsyncOneTimeSetUpTearDownAnalyzerTests
    {
        [Theory]
        [InlineData("OneTimeSetUpOnAsyncTask", "OneTimeSetUpAttribute", "UnityOneTimeSetUpAttribute")]
        [InlineData("OneTimeSetUpOnAsyncVoid", "OneTimeSetUpAttribute", "UnityOneTimeSetUpAttribute")]
        [InlineData("OneTimeTearDownOnNonAsyncTask", "OneTimeTearDownAttribute", "UnityOneTimeTearDownAttribute")]
        [InlineData("OneTimeTearDownOnGenericTask", "OneTimeTearDownAttribute", "UnityOneTimeTearDownAttribute")]
        public async Task AsyncOneTimeMethod_ReportsAtAttributeWithItsTypeNameAndReplacement(string fixture, string attributeName, string replacement)
        {
            await Verifier.VerifyAsync($"UTF1004/{fixture}.cs",
                Verifier.Diagnostic().WithLocation(8, 10).WithArguments(attributeName, replacement));
        }

        [Fact]
        public async Task BothAttributesOnAsyncTask_ReportsAtEachAttribute()
        {
            await Verifier.VerifyAsync("UTF1004/BothAttributesOnAsyncTask.cs",
                Verifier.Diagnostic().WithLocation(8, 10).WithArguments("OneTimeSetUpAttribute", "UnityOneTimeSetUpAttribute"),
                Verifier.Diagnostic().WithLocation(9, 10).WithArguments("OneTimeTearDownAttribute", "UnityOneTimeTearDownAttribute"));
        }

        [Theory]
        [InlineData("OneTimeSetUpOnSync")]
        [InlineData("SetUpOnAsyncTask")]
        [InlineData("UnityOneTimeSetUpOnCoroutine")]
        [InlineData("AsyncTaskWithoutAttribute")]
        [InlineData("DerivedAttributeOnAsyncTask")]
        [InlineData("OneTimeSetUpOnNonAsyncValueTask")]
        public async Task NotAsyncOrNotOneTimeAttribute_NoDiagnostic(string fixture)
        {
            await Verifier.VerifyAsync($"UTF1004/{fixture}.cs");
        }
    }
}
