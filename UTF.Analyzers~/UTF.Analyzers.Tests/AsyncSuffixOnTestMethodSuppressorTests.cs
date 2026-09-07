using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Verifier = UTF.Analyzers.Tests.TestDataVerifier<UTF.Analyzers.AsyncSuffixOnTestMethodSuppressor>;

namespace UTF.Analyzers.Tests
{
    public class AsyncSuffixOnTestMethodSuppressorTests
    {
        private static DiagnosticResult VSTHRD200(int line, int column)
        {
            return new DiagnosticResult(VSTHRD200StubAnalyzer.Rule).WithLocation(line, column);
        }

        private static DiagnosticResult[] TestMethodsDiagnostics(bool suppressed)
        {
            return new[]
            {
                VSTHRD200(10, 27).WithIsSuppressed(suppressed),
                VSTHRD200(16, 27).WithIsSuppressed(suppressed),
                VSTHRD200(23, 27).WithIsSuppressed(suppressed),
                VSTHRD200(29, 27).WithIsSuppressed(suppressed),
            };
        }

        [Fact]
        public async Task TestMethod_SuppressesVSTHRD200()
        {
            await Verifier.VerifyAsync(new Test(), "UTF3006/TestMethods.cs", TestMethodsDiagnostics(suppressed: true));
        }

        [Fact]
        public async Task CustomTestAttribute_SuppressesVSTHRD200()
        {
            await Verifier.VerifyAsync(new Test(), "UTF3006/CustomTestAttributes.cs",
                VSTHRD200(18, 27).WithIsSuppressed(true),
                VSTHRD200(24, 27).WithIsSuppressed(true));
        }

        [Fact]
        public async Task NonTestMethod_DoesNotSuppress()
        {
            await Verifier.VerifyAsync(new Test(), "UTF3006/NonTestMethods.cs",
                VSTHRD200(9, 27),
                VSTHRD200(15, 27),
                VSTHRD200(20, 27),
                VSTHRD200(26, 27).WithIsSuppressed(true),
                VSTHRD200(28, 24));
        }

        [Fact]
        public async Task DisabledByNoWarn_DoesNotSuppress()
        {
            var test = new Test
            {
                // Suppressors read only CompilationOptions.SpecificDiagnosticOptions (ruleset / -nowarn), not analyzer config files.
                SpecificDiagnosticOptions = ImmutableDictionary<string, ReportDiagnostic>.Empty
                    .Add(AsyncSuffixOnTestMethodSuppressor.SuppressionId, ReportDiagnostic.Suppress)
            };
            await Verifier.VerifyAsync(test, "UTF3006/TestMethods.cs", TestMethodsDiagnostics(suppressed: false));
        }

        /// <summary>
        /// Runs the VSTHRD200 stub together with the suppressor; a suppressor alone has nothing to suppress.
        /// </summary>
        private sealed class Test : Verifier.Test
        {
            protected override IEnumerable<DiagnosticAnalyzer> GetDiagnosticAnalyzers()
            {
                yield return new VSTHRD200StubAnalyzer();
                yield return new AsyncSuffixOnTestMethodSuppressor();
            }
        }
    }
}
