using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Verifier = UTF.Analyzers.Tests.TestDataVerifier<UTF.Analyzers.UnderscoreInTestMethodNameSuppressor>;

namespace UTF.Analyzers.Tests
{
    public class UnderscoreInTestMethodNameSuppressorTests
    {
        private static DiagnosticResult CA1707(int line, int column, string name)
        {
            return new DiagnosticResult(CA1707StubAnalyzer.Rule).WithLocation(line, column).WithArguments(name);
        }

        private static DiagnosticResult[] TestMethodsDiagnostics(bool suppressed)
        {
            return new[]
            {
                CA1707(10, 21, "Add_TwoPositiveNumbers_ReturnsSum").WithIsSuppressed(suppressed),
                CA1707(15, 28, "LoadScene_SceneExists_IsLoadedAfterOneFrame").WithIsSuppressed(suppressed),
                CA1707(22, 21, "IsPositive_PositiveNumber_ReturnsTrue").WithIsSuppressed(suppressed),
                CA1707(27, 21, "IsPositive_FromSource_ReturnsTrue").WithIsSuppressed(suppressed),
            };
        }

        [Fact]
        public async Task TestMethod_SuppressesCA1707()
        {
            await Verifier.VerifyAsync(new Test(), "UTF3005/TestMethods.cs", TestMethodsDiagnostics(suppressed: true));
        }

        [Fact]
        public async Task CustomTestAttribute_SuppressesCA1707()
        {
            await Verifier.VerifyAsync(new Test(), "UTF3005/CustomTestAttributes.cs",
                CA1707(17, 21, "Add_TwoPositiveNumbers_ReturnsSum").WithIsSuppressed(true),
                CA1707(22, 21, "Add_TwoNegativeNumbers_ReturnsSum").WithIsSuppressed(true));
        }

        [Fact]
        public async Task NonTestMethod_DoesNotSuppress()
        {
            await Verifier.VerifyAsync(new Test(), "UTF3005/NonTestMethods.cs",
                CA1707(10, 21, "Set_Up"),
                CA1707(15, 28, "Unity_SetUp"),
                CA1707(20, 21, "Create_Fixture"));
        }

        [Fact]
        public async Task DisabledByNoWarn_DoesNotSuppress()
        {
            var test = new Test
            {
                // Suppressors read only CompilationOptions.SpecificDiagnosticOptions (ruleset / -nowarn), not analyzer config files.
                SpecificDiagnosticOptions = ImmutableDictionary<string, ReportDiagnostic>.Empty
                    .Add(UnderscoreInTestMethodNameSuppressor.SuppressionId, ReportDiagnostic.Suppress)
            };
            await Verifier.VerifyAsync(test, "UTF3005/TestMethods.cs", TestMethodsDiagnostics(suppressed: false));
        }

        /// <summary>
        /// Runs the CA1707 stub together with the suppressor; a suppressor alone has nothing to suppress.
        /// </summary>
        private sealed class Test : Verifier.Test
        {
            public ImmutableDictionary<string, ReportDiagnostic> SpecificDiagnosticOptions { get; init; } =
                ImmutableDictionary<string, ReportDiagnostic>.Empty;

            protected override IEnumerable<DiagnosticAnalyzer> GetDiagnosticAnalyzers()
            {
                yield return new CA1707StubAnalyzer();
                yield return new UnderscoreInTestMethodNameSuppressor();
            }

            protected override CompilationOptions CreateCompilationOptions()
            {
                return base.CreateCompilationOptions().WithSpecificDiagnosticOptions(SpecificDiagnosticOptions);
            }
        }
    }
}
