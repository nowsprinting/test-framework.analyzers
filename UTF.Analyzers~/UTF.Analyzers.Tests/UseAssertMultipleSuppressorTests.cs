using System.Collections.Generic;
using System.Threading.Tasks;
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

        /// <summary>
        /// Runs the NUnit2045 stub together with the suppressor; a suppressor alone has nothing to suppress.
        /// </summary>
        private sealed class Test : Verifier.Test
        {
            protected override IEnumerable<DiagnosticAnalyzer> GetDiagnosticAnalyzers()
            {
                yield return new NUnit2045StubAnalyzer();
                yield return new UseAssertMultipleSuppressor();
            }
        }
    }
}
