using System.Threading.Tasks;
using Xunit;
using Verifier = UTF.Analyzers.Tests.TestDataVerifier<UTF.Analyzers.NonEnumeratorUnityTestMethodAnalyzer>;

namespace UTF.Analyzers.Tests
{
    public class NonEnumeratorUnityTestMethodAnalyzerTests
    {
        [Theory]
        [InlineData("GenericEnumeratorOnUnityTest", 9, "IEnumerator<object>")]
        [InlineData("EnumerableOnUnityTest", 9, "IEnumerable")]
        [InlineData("AsyncTaskOnUnityTest", 9, "Task")]
        [InlineData("VoidOnUnityTest", 8, "void")]
        [InlineData("AsyncTaskOnTestAndUnityTest", 11, "Task")]
        public async Task NonEnumeratorUnityTestMethod_ReportsAtReturnTypeWithItsName(string fixture, int line,
            string returnType)
        {
            await Verifier.VerifyAsync($"UTF1008/{fixture}.cs",
                Verifier.Diagnostic().WithLocation(line, 16).WithArguments(returnType));
        }

        [Theory]
        [InlineData("EnumeratorOnUnityTest")]
        [InlineData("AsyncTaskOnTest")]
        [InlineData("GenericEnumeratorWithoutUnityTest")]
        public async Task EnumeratorOrNotUnityTestMethod_NoDiagnostic(string fixture)
        {
            await Verifier.VerifyAsync($"UTF1008/{fixture}.cs");
        }
    }
}
