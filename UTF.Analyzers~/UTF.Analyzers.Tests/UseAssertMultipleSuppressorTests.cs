using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace UTF.Analyzers.Tests
{
    public class UseAssertMultipleSuppressorTests
    {
        [Fact]
        public async Task AssertMultipleNotDefined_SuppressesNUnit2045()
        {
            await VerifyAsync("UTF3001/TwoIndependentAssertions.cs",
                new DiagnosticResult(NUnit2045StubAnalyzer.Rule).WithLocation(12, 13).WithIsSuppressed(true),
                new DiagnosticResult(NUnit2045StubAnalyzer.Rule).WithLocation(13, 13).WithIsSuppressed(true));
        }

        private static Task VerifyAsync(string testDataPath, params DiagnosticResult[] expected)
        {
            var test = new Test { TestCode = File.ReadAllText(Path.Combine(TestDataFiles.Root, testDataPath)) };
            foreach (var dummy in TestDataFiles.Dummies)
            {
                test.TestState.Sources.Add(dummy);
            }

            test.ExpectedDiagnostics.AddRange(expected);
            return test.RunAsync();
        }

        /// <summary>
        /// Runs the NUnit2045 stub together with the suppressor. TestDataVerifier.Test runs a single analyzer,
        /// and a suppressor alone has nothing to suppress.
        /// </summary>
        private sealed class Test : TestDataVerifier<UseAssertMultipleSuppressor>.Test
        {
            protected override IEnumerable<DiagnosticAnalyzer> GetDiagnosticAnalyzers()
            {
                yield return new NUnit2045StubAnalyzer();
                yield return new UseAssertMultipleSuppressor();
            }
        }
    }
}
