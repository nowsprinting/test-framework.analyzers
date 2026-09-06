using System.Threading.Tasks;
using Xunit;
using Verifier = UTF.Analyzers.Tests.TestDataVerifier<UTF.Analyzers.DelayedConstraintAnalyzer>;

namespace UTF.Analyzers.Tests
{
    public class DelayedConstraintAnalyzerTests
    {
        [Theory]
        [InlineData("AfterPolling", 12, 55)]
        [InlineData("AfterSingle", 12, 55)]
        [InlineData("PlainValue", 12, 49)]
        [InlineData("AfterInLocal", 12, 47)]
        [InlineData("AfterOnCustomConstraint", 13, 61)]
        [InlineData("ExplicitConstruction", 13, 38)]
        public async Task DelayedConstraint_ReportsAtCreation(string fixture, int line, int column)
        {
            await Verifier.VerifyAsync($"UTF2005/{fixture}.cs", Verifier.Diagnostic().WithLocation(line, column));
        }

        [Theory]
        [InlineData("NoDelayGood")]
        [InlineData("UserAfterMethod")]
        public async Task NoDelayedConstraint_NoDiagnostic(string fixture)
        {
            await Verifier.VerifyAsync($"UTF2005/{fixture}.cs");
        }
    }
}
