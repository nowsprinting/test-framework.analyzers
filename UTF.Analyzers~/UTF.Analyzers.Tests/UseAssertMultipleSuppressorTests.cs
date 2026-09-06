using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Verifier = UTF.Analyzers.Tests.TestDataVerifier<UTF.Analyzers.UseAssertMultipleSuppressor>;

namespace UTF.Analyzers.Tests
{
    public class UseAssertMultipleSuppressorTests
    {
        [Fact]
        public async Task AssertMultipleNotDefined_SuppressesNUnit2045()
        {
            await Verifier.VerifyAsync(new Test(), "UTF3001/TwoIndependentAssertions.cs",
                new DiagnosticResult(NUnit2045StubAnalyzer.Rule).WithLocation(12, 13).WithIsSuppressed(true),
                new DiagnosticResult(NUnit2045StubAnalyzer.Rule).WithLocation(13, 13).WithIsSuppressed(true));
        }

        [Fact]
        public async Task DisabledByNoWarn_DoesNotSuppress()
        {
            var test = new Test
            {
                // Suppressors read only CompilationOptions.SpecificDiagnosticOptions (ruleset / -nowarn), not analyzer config files.
                SpecificDiagnosticOptions = ImmutableDictionary<string, ReportDiagnostic>.Empty
                    .Add(UseAssertMultipleSuppressor.SuppressionId, ReportDiagnostic.Suppress)
            };
            await Verifier.VerifyAsync(test, "UTF3001/TwoIndependentAssertions.cs",
                new DiagnosticResult(NUnit2045StubAnalyzer.Rule).WithLocation(12, 13),
                new DiagnosticResult(NUnit2045StubAnalyzer.Rule).WithLocation(13, 13));
        }

        /// <summary>
        /// Runs the NUnit2045 stub together with the suppressor; a suppressor alone has nothing to suppress.
        /// </summary>
        private sealed class Test : Verifier.Test
        {
            public ImmutableDictionary<string, ReportDiagnostic> SpecificDiagnosticOptions { get; init; } =
                ImmutableDictionary<string, ReportDiagnostic>.Empty;

            protected override IEnumerable<DiagnosticAnalyzer> GetDiagnosticAnalyzers()
            {
                yield return new NUnit2045StubAnalyzer();
                yield return new UseAssertMultipleSuppressor();
            }

            protected override CompilationOptions CreateCompilationOptions()
            {
                return base.CreateCompilationOptions().WithSpecificDiagnosticOptions(SpecificDiagnosticOptions);
            }
        }
    }
}
