using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Verifier = UTF.Analyzers.Tests.TestDataVerifier<UTF.Analyzers.NonNullableMemberInitializedInUnitySetUpSuppressor>;

namespace UTF.Analyzers.Tests
{
    public class NonNullableMemberInitializedInUnitySetUpSuppressorTests
    {
        // Collecting warnings also surfaces CS1591 (missing XML comment) on every public dummy and fixture member.
        private static readonly ImmutableDictionary<string, ReportDiagnostic> IgnoreMissingXmlComments =
            ImmutableDictionary<string, ReportDiagnostic>.Empty.Add("CS1591", ReportDiagnostic.Suppress);

        private static DiagnosticResult CS8618(int line, int column)
        {
            // The compiler attaches the same span again as an additional location.
            return DiagnosticResult.CompilerWarning(NonNullableMemberInitializedInUnitySetUpSuppressor.SuppressedDiagnosticId)
                .WithLocation(line, column)
                .WithOptions(DiagnosticOptions.IgnoreAdditionalLocations);
        }

        [Theory]
        [InlineData("UTF3002/FieldAssignedInUnitySetUp.cs", 9, 24)]
        [InlineData("UTF3002/PropertyAssignedInUnityOneTimeSetUp.cs", 9, 24)]
        [InlineData("UTF3002/FieldAssignedInHelperCalledFromUnitySetUp.cs", 9, 24)]
        [InlineData("UTF3002/FieldAssignedInOverriddenUnitySetUp.cs", 15, 24)]
        public async Task AssignedInUnitySetUp_SuppressesCS8618(string path, int line, int column)
        {
            await Verifier.VerifyAsync(new Test { SpecificDiagnosticOptions = IgnoreMissingXmlComments }, path, CS8618(line, column).WithIsSuppressed(true));
        }

        [Theory]
        [InlineData("UTF3002/FieldAssignedConditionally.cs", 9, 24)]
        [InlineData("UTF3002/FieldAssignedInSetUp.cs", 7, 24)]
        public async Task NotAssignedInUnitySetUp_DoesNotSuppress(string path, int line, int column)
        {
            await Verifier.VerifyAsync(new Test { SpecificDiagnosticOptions = IgnoreMissingXmlComments }, path, CS8618(line, column));
        }

        [Fact]
        public async Task DisabledByNoWarn_DoesNotSuppress()
        {
            var test = new Test
            {
                // Suppressors read only CompilationOptions.SpecificDiagnosticOptions (ruleset / -nowarn), not analyzer config files.
                SpecificDiagnosticOptions = IgnoreMissingXmlComments
                    .Add(NonNullableMemberInitializedInUnitySetUpSuppressor.SuppressionId, ReportDiagnostic.Suppress)
            };
            await Verifier.VerifyAsync(test, "UTF3002/FieldAssignedInUnitySetUp.cs", CS8618(9, 24));
        }

        /// <summary>
        /// CS8618 comes from the compiler itself, so no producer analyzer is needed alongside the suppressor.
        /// </summary>
        private sealed class Test : Verifier.Test
        {
            public Test()
            {
                // The verifier collects only compiler errors by default; CS8618 is a warning.
                CompilerDiagnostics = CompilerDiagnostics.Warnings;
            }
        }
    }
}
