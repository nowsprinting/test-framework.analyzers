using System.Threading.Tasks;
using Xunit;
using Verifier = UTF.Analyzers.Tests.TestDataVerifier<UTF.Analyzers.NonTestDelegateInAllocatingGCMemoryAnalyzer>;

namespace UTF.Analyzers.Tests
{
    public class NonTestDelegateInAllocatingGCMemoryAnalyzerTests
    {
        [Theory]
        [InlineData("ValueLambdaWithNot", 15, "ActualValueDelegate<bool>")]
        [InlineData("BoolMethodGroup", 15, "ActualValueDelegate<bool>")]
        [InlineData("ActionVariable", 17, "Action")]
        [InlineData("FuncVariable", 17, "Func<bool>")]
        [InlineData("AssumeValueLambda", 15, "ActualValueDelegate<bool>")]
        [InlineData("UnnegatedConstraint", 15, "ActualValueDelegate<bool>")]
        [InlineData("AfterChain", 15, "ActualValueDelegate<bool>")]
        [InlineData("NewConstraint", 15, "ActualValueDelegate<bool>")]
        [InlineData("WithMessage", 15, "ActualValueDelegate<bool>")]
        [InlineData("TaskValue", 16, "Task<int>")]
        [InlineData("ConstraintInVariable", 16, "ActualValueDelegate<bool>")]
        public async Task NonTestDelegateActual_ReportsAtActualArgumentWithBoundType(string fixture, int line,
            string boundType)
        {
            await Verifier.VerifyAsync($"UTF2004/{fixture}.cs",
                Verifier.Diagnostic().WithLocation(line, 25).WithArguments(boundType));
        }

        [Fact]
        public async Task ConstraintInOuterBlockVariable_ReportsAtActualArgument()
        {
            await Verifier.VerifyAsync("UTF2004/ConstraintInOuterBlockVariable.cs",
                Verifier.Diagnostic().WithLocation(18, 29).WithArguments("ActualValueDelegate<bool>"));
        }

        [Theory]
        [InlineData("GoodShapes")]
        [InlineData("AsyncLambdaOwnedByUTF2003")]
        [InlineData("ThrowsNothingAndExtensionChain")]
        [InlineData("OtherConstraint")]
        public async Task TestDelegateOrOutOfScope_NoDiagnostic(string fixture)
        {
            await Verifier.VerifyAsync($"UTF2004/{fixture}.cs");
        }
    }
}
