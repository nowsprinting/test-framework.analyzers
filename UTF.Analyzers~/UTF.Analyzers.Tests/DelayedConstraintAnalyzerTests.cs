using System.Threading.Tasks;
using Xunit;
using Verifier = UTF.Analyzers.Tests.TestDataVerifier<UTF.Analyzers.DelayedConstraintAnalyzer>;

namespace UTF.Analyzers.Tests
{
    public class DelayedConstraintAnalyzerTests
    {
        [Theory]
        [InlineData("AfterPolling", 12, 38)]
        [InlineData("AfterSingle", 12, 38)]
        [InlineData("PlainValue", 12, 32)]
        [InlineData("AfterInLocal", 12, 30)]
        [InlineData("AfterOnCustomConstraint", 13, 38)]
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
