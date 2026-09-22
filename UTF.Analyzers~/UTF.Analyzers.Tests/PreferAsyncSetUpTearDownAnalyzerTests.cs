using System.Threading.Tasks;
using Xunit;
using Verifier = UTF.Analyzers.Tests.TestDataVerifier<UTF.Analyzers.PreferAsyncSetUpTearDownAnalyzer>;

namespace UTF.Analyzers.Tests
{
    public class PreferAsyncSetUpTearDownAnalyzerTests
    {
        [Theory]
        [InlineData("UnitySetUpYieldNull", 11, "SetUp", "UnitySetUpAttribute", "SetUpAttribute")]
        [InlineData("YieldFixtureHelper", 11, "SetUp", "UnitySetUpAttribute", "SetUpAttribute")]
        [InlineData("OverriddenBaseDeclaration", 16, "SetUp", "UnitySetUpAttribute", "SetUpAttribute")]
        [InlineData("UnityTearDownYieldWaitUntil", 13, "TearDown", "UnityTearDownAttribute", "TearDownAttribute")]
        [InlineData("NoYieldReturn", 11, "TearDown", "UnityTearDownAttribute", "TearDownAttribute")]
        public async Task ConvertibleCoroutineHook_ReportsAtReturnTypeAndName(string fixture, int line, string name,
            string attribute, string replacement)
        {
            var expected = Verifier.Diagnostic().WithLocation(line, 16).WithArguments(name, attribute, replacement);
            await Verifier.VerifyAsync($"UTF4007/{fixture}.cs", expected);
        }

        [Theory]
        [InlineData("YieldEnterPlayMode")]
        [InlineData("YieldCoroutineUnderTest")]
        [InlineData("UnityOneTimeSetUp")]
        [InlineData("UnityTestMethod")]
        [InlineData("TaskReturningUnitySetUp")]
        [InlineData("AsyncSetUp")]
        public async Task NotConvertibleOrNotUnitySetUpTearDown_NoDiagnostic(string fixture)
        {
            await Verifier.VerifyAsync($"UTF4007/{fixture}.cs");
        }
    }
}
