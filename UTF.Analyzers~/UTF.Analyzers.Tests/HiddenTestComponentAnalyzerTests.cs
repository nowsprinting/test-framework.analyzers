using System.IO;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Verifier = UTF.Analyzers.Tests.TestDataVerifier<UTF.Analyzers.HiddenTestComponentAnalyzer>;

namespace UTF.Analyzers.Tests
{
    public class HiddenTestComponentAnalyzerTests
    {
        private const string TestsAssembly = "MyGame.Tests";
        private const string RuntimeAssembly = "MyGame";
        private const string TestsDirectory = "/Assets/Tests/";
        private const string RuntimeDirectory = "/Assets/Scripts/";

        [Theory]
        [InlineData("NoAttribute", 5, 18)]
        [InlineData("EmptyMenuName", 6, 18)]
        [InlineData("VisibleMenuName", 6, 18)]
        [InlineData("DerivedFromHiddenBase", 10, 18)]
        [InlineData("PartialClass", 5, 26)]
        [InlineData("NameMatchesFileAmongOthers", 10, 18)]
        public async Task VisibleComponentInTestsAssembly_Reports(string fixture, int line, int column)
        {
            var path = RuntimeDirectory + fixture + ".cs";
            var expected = Verifier.Diagnostic().WithLocation(path, line, column).WithArguments(fixture);
            await VerifyAsync(fixture, path, TestsAssembly, expected);
        }

        [Fact]
        public async Task VisibleComponentUnderTestsDirectory_Reports()
        {
            var path = TestsDirectory + "NoAttribute.cs";
            var expected = Verifier.Diagnostic().WithLocation(path, 5, 18).WithArguments("NoAttribute");
            await VerifyAsync("NoAttribute", path, RuntimeAssembly, expected);
        }

        [Theory]
        [InlineData("/Packages/com.example.foo/Tests/Runtime/NoAttribute.cs")]
        [InlineData(@"C:\Library\PackageCache\com.example.foo@1.0.0\Tests\Editor\NoAttribute.cs")]
        public async Task TestsDirectoryWithEitherSeparator_Reports(string path)
        {
            var expected = Verifier.Diagnostic().WithLocation(path, 5, 18).WithArguments("NoAttribute");
            await VerifyAsync("NoAttribute", path, RuntimeAssembly, expected);
        }

        [Theory]
        [InlineData("/Assets/TestsOfFoo/NoAttribute.cs")]
        [InlineData("/Assets/tests/NoAttribute.cs")]
        [InlineData("/Assets/Tests.Old/NoAttribute.cs")]
        public async Task DirectoryNotNamedTests_NoDiagnostic(string path)
        {
            await VerifyAsync("NoAttribute", path, RuntimeAssembly);
        }

        [Theory]
        [InlineData("MyGame")]
        [InlineData("Tests")]
        [InlineData("MyGame.TestsSupport")]
        public async Task AssemblyNameNotEndingWithTests_NoDiagnostic(string assemblyName)
        {
            await VerifyAsync("NoAttribute", RuntimeDirectory + "NoAttribute.cs", assemblyName);
        }

        [Theory]
        [InlineData("Hidden")]
        [InlineData("HiddenWithPath")]
        [InlineData("HiddenOnOtherPartialDeclaration")]
        [InlineData("AbstractClass")]
        [InlineData("GenericClass")]
        [InlineData("NestedClass")]
        [InlineData("NotMonoBehaviour")]
        [InlineData("NameDiffersFromFileAmongOthers")]
        public async Task HiddenOrNotAttachable_NoDiagnostic(string fixture)
        {
            await VerifyAsync(fixture, TestsDirectory + fixture + ".cs", TestsAssembly);
        }

        [Fact]
        public async Task PartialDeclarationInFileNotNamedAfterClass_NoDiagnostic()
        {
            await VerifyAsync("PartialClass", TestsDirectory + "PartialClass.Other.cs", TestsAssembly);
        }

        /// <summary>
        /// The harness adds the fixture as "/0/Test0.cs" in project "TestProject", so neither path nor assembly name can mark
        /// a test assembly; this rule needs both under control, hence the explicit path and project name here.
        /// </summary>
        private static Task VerifyAsync(string fixture, string path, string assemblyName, params DiagnosticResult[] expected)
        {
            var test = new NamedProjectTest(assemblyName);
            test.TestState.Sources.Add((path, File.ReadAllText(Path.Combine(TestDataFiles.Root, "UTF4005", fixture + ".cs"))));
            foreach (var dummy in TestDataFiles.Dummies)
            {
                test.TestState.Sources.Add(dummy);
            }

            test.ExpectedDiagnostics.AddRange(expected);
            return test.RunAsync();
        }

        private sealed class NamedProjectTest : Verifier.Test
        {
            public NamedProjectTest(string assemblyName)
            {
                // DefaultTestProjectName is read by the base constructor, before a subclass constructor can set it,
                // and the harness looks the project up by that name afterwards, so only the assembly name is changed.
                SolutionTransforms.Add((solution, projectId) => solution.WithProjectAssemblyName(projectId, assemblyName));
            }
        }
    }
}
