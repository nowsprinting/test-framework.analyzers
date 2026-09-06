using System.Threading.Tasks;
using Xunit;
using Verifier = UTF.Analyzers.Tests.TestDataVerifier<UTF.Analyzers.ExceptionInAttributeHookAnalyzer>;

namespace UTF.Analyzers.Tests
{
    public class ExceptionInAttributeHookAnalyzerTests
    {
        private const string ApplyToTest = "IApplyToTest.ApplyToTest";
        private const string ApplyToContext = "IApplyToContext.ApplyToContext";
        private const string Wrap = "ICommandWrapper.Wrap";

        [Theory]
        [InlineData("ThrowInApplyToTest", 13, 13, ApplyToTest)]
        [InlineData("ThrowInApplyToContext", 13, 13, ApplyToContext)]
        [InlineData("ThrowExpressionInWrap", 13, 48, Wrap)]
        [InlineData("ThrowInWrapSetUpTearDown", 13, 13, Wrap)]
        [InlineData("ExplicitImplementation", 13, 13, ApplyToTest)]
        [InlineData("OverrideOfImplementation", 13, 13, ApplyToTest)]
        [InlineData("InheritedImplementation", 17, 13, ApplyToTest)]
        [InlineData("Rethrow", 19, 17, ApplyToTest)]
        [InlineData("ThrowInCatchHandler", 19, 17, ApplyToTest)]
        [InlineData("ThrowInFinally", 19, 17, ApplyToTest)]
        [InlineData("CatchWithFilter", 15, 17, ApplyToTest)]
        [InlineData("CatchOfUnrelatedType", 15, 17, ApplyToTest)]
        [InlineData("CatchOfDerivedType", 15, 17, ApplyToTest)]
        public async Task ThrowEscapesFromHook_ReportsAtThrow(string fixture, int line, int column, string member)
        {
            var expected = Verifier.Diagnostic().WithLocation(line, column).WithArguments(member);
            await Verifier.VerifyAsync($"UTF5001/{fixture}.cs", expected);
        }

        [Theory]
        [InlineData("CallToThrowingMethod", 13, 39, ApplyToContext)]
        [InlineData("CallToThrowingConstructor", 13, 20, Wrap)]
        [InlineData("CallToConstructorWithThrowingInitializer", 13, 20, Wrap)]
        [InlineData("CallToThrowingLocalFunction", 13, 13, ApplyToTest)]
        [InlineData("TransitiveCall", 13, 13, ApplyToTest)]
        [InlineData("PartiallyCaughtCallee", 15, 17, ApplyToTest)]
        [InlineData("RecursiveCallee", 13, 13, ApplyToTest)]
        public async Task ThrowEscapesFromCallee_ReportsAtCallSite(string fixture, int line, int column, string member)
        {
            var expected = Verifier.Diagnostic().WithLocation(line, column).WithArguments(member);
            await Verifier.VerifyAsync($"UTF5001/{fixture}.cs", expected);
        }

        [Fact]
        public async Task MultipleEscapeSites_ReportsEachSite()
        {
            var expected = new[]
            {
                Verifier.Diagnostic().WithLocation(15, 17).WithArguments(ApplyToTest),
                Verifier.Diagnostic().WithLocation(18, 13).WithArguments(ApplyToTest),
            };
            await Verifier.VerifyAsync("UTF5001/MultipleSites.cs", expected);
        }

        [Theory]
        [InlineData("GoodExample")]
        [InlineData("GeneralCatch")]
        [InlineData("BaseTypeCatch")]
        [InlineData("RethrowInsideOuterTry")]
        [InlineData("CalleeCatchesInternally")]
        [InlineData("CallerCatchesCallee")]
        [InlineData("ThrowInLambda")]
        [InlineData("UncalledLocalFunction")]
        [InlineData("ExternalMethod")]
        [InlineData("ThrowInPropertyGetter")]
        [InlineData("AsyncAndIteratorCallee")]
        [InlineData("NonImplementingMethod")]
        [InlineData("RecursionWithoutThrow")]
        public async Task HandledOrUntracked_NoDiagnostic(string fixture)
        {
            await Verifier.VerifyAsync($"UTF5001/{fixture}.cs");
        }
    }
}
