using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Verifier = UTF.Analyzers.Tests.TestDataVerifier<UTF.Analyzers.UnityHookMethodIsPublicSuppressor>;

namespace UTF.Analyzers.Tests
{
    public class UnityHookMethodIsPublicSuppressorTests
    {
        private static DiagnosticResult NUnit1028(int line, int column)
        {
            return new DiagnosticResult(NUnit1028StubAnalyzer.Rule).WithLocation(line, column);
        }

        [Theory]
        [InlineData("UTF3004/PublicUnitySetUpAndTearDown.cs")]
        [InlineData("UTF3004/PublicUnityOneTimeSetUpAndTearDown.cs")]
        public async Task UnityHookMethod_SuppressesNUnit1028(string path)
        {
            await Verifier.VerifyAsync(new Test(), path,
                NUnit1028(10, 28).WithIsSuppressed(true),
                NUnit1028(16, 28).WithIsSuppressed(true));
        }

        [Fact]
        public async Task OverriddenUnityHookMethod_SuppressesNUnit1028()
        {
            // The base class has no test-related method, so NUnit1028 is not reported there.
            await Verifier.VerifyAsync(new Test(), "UTF3004/OverriddenUnitySetUp.cs",
                NUnit1028(15, 37).WithIsSuppressed(true));
        }

        [Fact]
        public async Task NoUnityHookAttribute_DoesNotSuppress()
        {
            await Verifier.VerifyAsync(new Test(), "UTF3004/PublicHelper.cs", NUnit1028(7, 21));
        }

        [Fact]
        public async Task DisabledByNoWarn_DoesNotSuppress()
        {
            var test = new Test
            {
                // Suppressors read only CompilationOptions.SpecificDiagnosticOptions (ruleset / -nowarn), not analyzer config files.
                SpecificDiagnosticOptions = ImmutableDictionary<string, ReportDiagnostic>.Empty
                    .Add(UnityHookMethodIsPublicSuppressor.SuppressionId, ReportDiagnostic.Suppress)
            };
            await Verifier.VerifyAsync(test, "UTF3004/PublicUnitySetUpAndTearDown.cs", NUnit1028(10, 28),
                NUnit1028(16, 28));
        }

        /// <summary>
        /// Runs the NUnit1028 stub together with the suppressor; a suppressor alone has nothing to suppress.
        /// </summary>
        private sealed class Test : Verifier.Test
        {
            public ImmutableDictionary<string, ReportDiagnostic> SpecificDiagnosticOptions { get; init; } =
                ImmutableDictionary<string, ReportDiagnostic>.Empty;

            protected override IEnumerable<DiagnosticAnalyzer> GetDiagnosticAnalyzers()
            {
                yield return new NUnit1028StubAnalyzer();
                yield return new UnityHookMethodIsPublicSuppressor();
            }

            protected override CompilationOptions CreateCompilationOptions()
            {
                return base.CreateCompilationOptions().WithSpecificDiagnosticOptions(SpecificDiagnosticOptions);
            }
        }
    }
}
