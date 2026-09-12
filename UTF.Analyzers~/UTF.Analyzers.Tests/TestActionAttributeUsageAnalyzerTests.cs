using System.Threading.Tasks;
using Xunit;
using Verifier = UTF.Analyzers.Tests.TestDataVerifier<UTF.Analyzers.TestActionAttributeUsageAnalyzer>;

namespace UTF.Analyzers.Tests
{
    public class TestActionAttributeUsageAnalyzerTests
    {
        private const string Test = "'ActionTargets.Test'";
        private const string Suite = "'ActionTargets.Suite'";
        private const string TestMethods = "test methods";
        private const string SuiteTargets = "fixture classes, interfaces, and assemblies";

        [Theory]
        [InlineData("TestNoUsage", 7, 48, Test, TestMethods)]
        [InlineData("GetterBlock", 7, 48, Test, TestMethods)]
        [InlineData("ExplicitInterface", 7, 54, Test, TestMethods)]
        [InlineData("PartialClass", 12, 41, Test, TestMethods)]
        [InlineData("SuiteAllowsMethod", 8, 54, Suite, SuiteTargets)]
        [InlineData("DefaultFromTestActionAttribute", 7, 18, "'ActionTargets.Default'", SuiteTargets)]
        [InlineData("BothFlagsNoUsage", 7, 53, "'ActionTargets.Test | ActionTargets.Suite'", "test methods, fixture classes, interfaces, and assemblies")]
        [InlineData("UnknownMember", 7, 50, "not a constant", "test methods and fixture classes")]
        [InlineData("NonConstantAllowsAssembly", 8, 62, "not a constant", "test methods and fixture classes")]
        public async Task AttributeAllowingUnsupportedTargets_ReportsWithTargetsValue(string fixture, int line, int column,
            string targets, string supported)
        {
            var expected = Verifier.Diagnostic().WithLocation(line, column).WithArguments(targets, supported);
            await Verifier.VerifyAsync($"UTF5004/{fixture}.cs", expected);
        }

        [Fact]
        public async Task TargetsInheritedFromSourceBase_ReportsBaseAtEntryAndDerivedAtClassName()
        {
            var expected = new[]
            {
                Verifier.Diagnostic().WithLocation(7, 18).WithArguments(Suite, SuiteTargets),
                Verifier.Diagnostic().WithLocation(12, 85).WithArguments(Suite, SuiteTargets),
            };
            await Verifier.VerifyAsync("UTF5004/TargetsInheritedFromSourceBase.cs", expected);
        }

        [Theory]
        [InlineData("TestMethodOnly")]
        [InlineData("SuiteClassInterfaceAssembly")]
        [InlineData("BothFlagsAllFour")]
        [InlineData("NonConstantMethodClass")]
        [InlineData("NarrowedTestActionAttribute")]
        [InlineData("NonAttributeClass")]
        public async Task SupportedTargetsOrNonAttribute_NoDiagnostic(string fixture)
        {
            await Verifier.VerifyAsync($"UTF5004/{fixture}.cs");
        }
    }
}
