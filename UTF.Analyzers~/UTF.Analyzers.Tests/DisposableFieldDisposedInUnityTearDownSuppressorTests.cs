using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using UTF.Analyzers.Tests.StubAnalyzers;
using Xunit;
using Verifier = UTF.Analyzers.Tests.TestDataVerifier<UTF.Analyzers.DisposableFieldDisposedInUnityTearDownSuppressor>;

namespace UTF.Analyzers.Tests
{
    public class DisposableFieldDisposedInUnityTearDownSuppressorTests
    {
        private static DiagnosticResult CA1001(int line, int column)
        {
            return new DiagnosticResult(CA1001StubAnalyzer.Rule).WithLocation(line, column);
        }

        [Theory]
        [InlineData("UTF3003/FieldDisposedInUnityTearDown.cs", 7, 18)]
        [InlineData("UTF3003/FieldDisposedInUnityOneTimeTearDown.cs", 7, 18)]
        [InlineData("UTF3003/FieldDisposedInOverriddenUnityTearDown.cs", 13, 18)]
        public async Task HasUnityTearDown_SuppressesCA1001(string path, int line, int column)
        {
            await Verifier.VerifyAsync(new Test(), path, CA1001(line, column).WithIsSuppressed(true));
        }

        [Theory]
        [InlineData("UTF3003/NoUnityTearDown.cs", 7, 18)]
        [InlineData("UTF3003/FieldDisposedInTearDown.cs", 6, 18)]
        public async Task NoUnityTearDown_DoesNotSuppress(string path, int line, int column)
        {
            await Verifier.VerifyAsync(new Test(), path, CA1001(line, column));
        }

        [Fact]
        public async Task DisabledByNoWarn_DoesNotSuppress()
        {
            var test = new Test
            {
                // Suppressors read only CompilationOptions.SpecificDiagnosticOptions (ruleset / -nowarn), not analyzer config files.
                SpecificDiagnosticOptions = ImmutableDictionary<string, ReportDiagnostic>.Empty
                    .Add(DisposableFieldDisposedInUnityTearDownSuppressor.SuppressionId, ReportDiagnostic.Suppress)
            };
            await Verifier.VerifyAsync(test, "UTF3003/FieldDisposedInUnityTearDown.cs", CA1001(7, 18));
        }

        /// <summary>
        /// Runs the CA1001 stub together with the suppressor; a suppressor alone has nothing to suppress.
        /// </summary>
        private sealed class Test : Verifier.Test
        {
            protected override IEnumerable<DiagnosticAnalyzer> GetDiagnosticAnalyzers()
            {
                yield return new CA1001StubAnalyzer();
                yield return new DisposableFieldDisposedInUnityTearDownSuppressor();
            }
        }
    }
}
