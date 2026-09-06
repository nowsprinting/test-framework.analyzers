using System.Threading.Tasks;
using Xunit;
using Verifier = UTF.Analyzers.Tests.TestDataVerifier<UTF.Analyzers.CombiningStrategyOnCoroutineAnalyzer>;

namespace UTF.Analyzers.Tests
{
    public class CombiningStrategyOnCoroutineAnalyzerTests
    {
        [Theory]
        [InlineData("UnityTestWithPairwiseOnCoroutine", "PairwiseAttribute", 10)]
        [InlineData("SequentialOnCoroutine", "SequentialAttribute", 8)]
        [InlineData("UnityTestWithCombinatorialOnCoroutine", "CombinatorialAttribute", 10)]
        public async Task IEnumeratorMethodWithCombiningStrategyAttribute_ReportsAtAttributeWithItsTypeName(string fixture, string attributeName, int line)
        {
            var expected = Verifier.Diagnostic().WithLocation(line, 10).WithArguments(attributeName);
            await Verifier.VerifyAsync($"UTF1003/{fixture}.cs", expected);
        }

        [Theory]
        [InlineData("PairwiseOnAsyncTask")]
        [InlineData("SequentialOnVoid")]
        [InlineData("UnityTestWithValuesOnCoroutine")]
        [InlineData("PairwiseOnGenericEnumerator")]
        [InlineData("PairwiseOnEnumerable")]
        public async Task NotCoroutineOrNoCombiningStrategyAttribute_NoDiagnostic(string fixture)
        {
            await Verifier.VerifyAsync($"UTF1003/{fixture}.cs");
        }
    }
}
