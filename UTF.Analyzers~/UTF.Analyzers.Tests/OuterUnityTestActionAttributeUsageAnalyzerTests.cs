using System.Threading.Tasks;
using Xunit;
using Verifier = UTF.Analyzers.Tests.TestDataVerifier<UTF.Analyzers.OuterUnityTestActionAttributeUsageAnalyzer>;

namespace UTF.Analyzers.Tests
{
    public class OuterUnityTestActionAttributeUsageAnalyzerTests
    {
        [Theory]
        [InlineData("NoAttributeUsage", 9, 53)]
        [InlineData("AllowsClass", 10, 48)]
        [InlineData("DerivedInterface", 13, 53)]
        [InlineData("QualifiedName", 7, 50)]
        [InlineData("PartialClass", 13, 41)]
        [InlineData("PartialWidensBase", 10, 26)]
        public async Task AttributeAllowingOtherTargets_ReportsOnce(string fixture, int line, int column)
        {
            var expected = Verifier.Diagnostic().WithLocation(line, column);
            await Verifier.VerifyAsync($"UTF5003/{fixture}.cs", expected);
        }

        [Fact]
        public async Task InterfaceInheritedFromBaseClass_ReportsBaseAtEntryAndDerivedAtClassName()
        {
            var expected = new[]
            {
                Verifier.Diagnostic().WithLocation(9, 18),
                Verifier.Diagnostic().WithLocation(14, 72),
            };
            await Verifier.VerifyAsync("UTF5003/InheritedFromBase.cs", expected);
        }

        [Theory]
        [InlineData("MethodOnly")]
        [InlineData("UsageInheritedFromBase")]
        [InlineData("NonAttributeClass")]
        public async Task MethodOnlyOrNonAttribute_NoDiagnostic(string fixture)
        {
            await Verifier.VerifyAsync($"UTF5003/{fixture}.cs");
        }
    }
}
