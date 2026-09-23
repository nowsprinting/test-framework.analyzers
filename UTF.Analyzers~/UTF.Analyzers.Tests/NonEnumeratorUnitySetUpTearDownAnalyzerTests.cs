using System.Threading.Tasks;
using Xunit;
using Verifier = UTF.Analyzers.Tests.TestDataVerifier<UTF.Analyzers.NonEnumeratorUnitySetUpTearDownAnalyzer>;

namespace UTF.Analyzers.Tests
{
    public class NonEnumeratorUnitySetUpTearDownAnalyzerTests
    {
        [Theory]
        [InlineData("UnitySetUpOnGenericEnumerator", 9, 16, "IEnumerator<object>", "UnitySetUpAttribute")]
        [InlineData("UnityTearDownOnEnumerable", 9, 16, "IEnumerable", "UnityTearDownAttribute")]
        [InlineData("UnityOneTimeSetUpOnAsyncTask", 9, 22, "Task", "UnityOneTimeSetUpAttribute")]
        [InlineData("UnityOneTimeTearDownOnVoid", 8, 16, "void", "UnityOneTimeTearDownAttribute")]
        [InlineData("UnityTearDownAndUnitySetUpOnEnumerable", 10, 16, "IEnumerable", "UnityTearDownAttribute")]
        [InlineData("SetUpAndUnitySetUpOnAsyncTask", 11, 22, "Task", "UnitySetUpAttribute")]
        [InlineData("OverrideOfUnitySetUpOnEnumerable", 9, 24, "IEnumerable", "UnitySetUpAttribute")]
        public async Task NonEnumeratorUnityHookMethod_ReportsOnceAtReturnTypeWithFirstAttribute(string fixture,
            int line, int column, string returnType, string attribute)
        {
            await Verifier.VerifyAsync($"UTF1009/{fixture}.cs",
                Verifier.Diagnostic().WithLocation(line, column).WithArguments(returnType, attribute));
        }

        [Theory]
        [InlineData("AllAttributesOnEnumerator")]
        [InlineData("EnumerableWithoutAttribute")]
        [InlineData("DerivedUnitySetUpOnEnumerable")]
        public async Task EnumeratorOrNotUnityHookMethod_NoDiagnostic(string fixture)
        {
            await Verifier.VerifyAsync($"UTF1009/{fixture}.cs");
        }
    }
}
