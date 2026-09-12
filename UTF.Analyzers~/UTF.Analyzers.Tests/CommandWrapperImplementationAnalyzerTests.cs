using System.Threading.Tasks;
using Xunit;
using Verifier = UTF.Analyzers.Tests.TestDataVerifier<UTF.Analyzers.CommandWrapperImplementationAnalyzer>;

namespace UTF.Analyzers.Tests
{
    /// <summary>
    /// The exempt types (RepeatAttribute, RetryAttribute, MaxTimeAttribute, ParametrizedIgnoreAttribute) are declared as dummies
    /// that implement IWrapTestMethod and are compiled into every case, so every case also verifies that they are not reported.
    /// </summary>
    public class CommandWrapperImplementationAnalyzerTests
    {
        [Theory]
        [InlineData("WrapTestMethod", 7, 51, "IWrapTestMethod")]
        [InlineData("DerivedInterface", 7, 53, "IDerivedInterfaceWrapper")]
        [InlineData("QualifiedName", 6, 50, "IWrapTestMethod")]
        [InlineData("NonAttributeClass", 6, 38, "IWrapTestMethod")]
        [InlineData("PartialClass", 11, 41, "IWrapTestMethod")]
        [InlineData("DerivedFromWrapperClass", 11, 73, "IWrapTestMethod")]
        public async Task ClassNamingWrapperInterface_ReportsAtBaseListEntryWithInterfaceName(string fixture, int line,
            int column, string interfaceName)
        {
            var expected = Verifier.Diagnostic().WithLocation(line, column).WithArguments(interfaceName);
            await Verifier.VerifyAsync($"UTF5002/{fixture}.cs", expected);
        }

        [Fact]
        public async Task ClassNamingBothInterfaces_ReportsEachEntry()
        {
            var expected = new[]
            {
                Verifier.Diagnostic().WithLocation(7, 51).WithArguments("IWrapTestMethod"),
                Verifier.Diagnostic().WithLocation(7, 68).WithArguments("IWrapSetUpTearDown"),
            };
            await Verifier.VerifyAsync("UTF5002/BothInterfaces.cs", expected);
        }

        [Theory]
        [InlineData("CommandWrapperOnly")]
        [InlineData("OuterUnityTestAction")]
        public async Task CommandWrapperOnlyOrOuterUnityTestAction_NoDiagnostic(string fixture)
        {
            await Verifier.VerifyAsync($"UTF5002/{fixture}.cs");
        }
    }
}
