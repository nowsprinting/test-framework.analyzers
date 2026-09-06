using System.Threading.Tasks;
using Xunit;
using Verifier = UTF.Analyzers.Tests.TestDataVerifier<UTF.Analyzers.NonTestDelegateInAllocatingGCMemoryAnalyzer>;

namespace UTF.Analyzers.Tests
{
    public class NonTestDelegateInAllocatingGCMemoryAnalyzerTests
    {
        [Theory]
        [InlineData("ValueLambdaWithNot", 17, "ActualValueDelegate<bool>")]
        [InlineData("BoolMethodGroup", 17, "ActualValueDelegate<bool>")]
        [InlineData("ActionVariable", 18, "Action")]
        [InlineData("FuncVariable", 18, "Func<bool>")]
        [InlineData("AssumeValueLambda", 17, "ActualValueDelegate<bool>")]
        [InlineData("UnnegatedConstraint", 17, "ActualValueDelegate<bool>")]
        [InlineData("AfterChain", 17, "ActualValueDelegate<bool>")]
        [InlineData("NewConstraint", 17, "ActualValueDelegate<bool>")]
        [InlineData("WithMessage", 17, "ActualValueDelegate<bool>")]
        [InlineData("TaskValue", 17, "Task<int>")]
        public async Task NonTestDelegateActual_ReportsAtActualArgumentWithBoundType(string fixture, int line,
            string boundType)
        {
            await Verifier.VerifyAsync($"UTF2004/{fixture}.cs",
                Verifier.Diagnostic().WithLocation(line, 25).WithArguments(boundType));
        }

        [Theory]
        [InlineData("GoodShapes")]
        [InlineData("AsyncLambdaOwnedByUTF2003")]
        [InlineData("OtherConstraint")]
        [InlineData("ConstraintInVariable")]
        public async Task TestDelegateOrOutOfScope_NoDiagnostic(string fixture)
        {
            await Verifier.VerifyAsync($"UTF2004/{fixture}.cs");
        }
    }
}
