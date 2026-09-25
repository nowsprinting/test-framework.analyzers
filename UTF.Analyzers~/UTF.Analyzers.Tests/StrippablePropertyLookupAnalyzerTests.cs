using System.IO;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Verifier = UTF.Analyzers.Tests.TestDataVerifier<UTF.Analyzers.StrippablePropertyLookupAnalyzer>;

namespace UTF.Analyzers.Tests
{
    public class StrippablePropertyLookupAnalyzerTests
    {
        [Theory]
        [InlineData("ThrowsTypeOfWithProperty", 19, 61, "ArgumentException.ParamName")]
        [InlineData("ThrowsArgumentExceptionWithProperty", 18, 74, "ArgumentException.ParamName")]
        [InlineData("HasPropertyExists", 13, 71, "ArgumentException.ParamName")]
        [InlineData("HasNoProperty", 13, 55, "FileInfo.Length")]
        [InlineData("NameofPropertyName", 13, 52, "FileInfo.Length")]
        [InlineData("UserTypeProperty", 18, 47, "ScoreBoard.Score")]
        [InlineData("HasSomePropertyOnArray", 14, 41, "FileInfo.Length")]
        [InlineData("HasExactlyProperty", 14, 47, "FileInfo.Length")]
        [InlineData("OrderedBy", 14, 43, "FileInfo.Length")]
        [InlineData("ListMapProperty", 14, 41, "FileInfo.Length")]
        [InlineData("MessageByName", 13, 68, "Exception.Message")]
        public async Task PropertyNamedByString_OnResolvedType_ReportsWithDeclaringType(string fixture, int line,
            int column, string property)
        {
            await Verifier.VerifyAsync($"UTF2006/{fixture}.cs",
                Verifier.Diagnostic().WithLocation(line, column).WithArguments(property));
        }

        [Theory]
        [InlineData("PropertyOnObjectActual", 14, 37, "Length")]
        [InlineData("PropertyOnTypeParameterActual", 13, 37, "Length")]
        [InlineData("PropertyOnSystemTypeActual", 13, 47, "IsInterface")]
        [InlineData("PropertyNotFoundOnStaticType", 20, 61, "ParamName")]
        [InlineData("NonexistentProperty", 13, 52, "NoSuchProperty")]
        [InlineData("PropertyInVariable", 13, 34, "Length")]
        [InlineData("NonConstantPropertyName", 14, 52, "name")]
        public async Task PropertyNamedByString_OnUnresolvedType_ReportsWithNameAsWritten(string fixture, int line,
            int column, string name)
        {
            await Verifier.VerifyAsync($"UTF2006/{fixture}.cs",
                Verifier.Diagnostic().WithLocation(line, column).WithArguments(name));
        }

        [Theory]
        [InlineData("HasLengthOnFileInfo", 13, 52)]
        [InlineData("ActualValueDelegate", 13, 58)]
        [InlineData("AssumeThat", 13, 52)]
        [InlineData("HasAllLengthOnList", 14, 40)]
        [InlineData("IsTypeOfAndProperty", 14, 59)]
        public async Task ShorthandOnStrippableProperty_ReportsResolvedProperty(string fixture, int line, int column)
        {
            await Verifier.VerifyAsync($"UTF2006/{fixture}.cs",
                Verifier.Diagnostic().WithLocation(line, column).WithArguments("FileInfo.Length"));
        }

        [Fact]
        public async Task OrderedThenBy_ReportsEachProperty()
        {
            await Verifier.VerifyAsync("UTF2006/OrderedThenBy.cs",
                Verifier.Diagnostic().WithLocation(14, 43).WithArguments("FileInfo.DirectoryName"),
                Verifier.Diagnostic().WithLocation(14, 68).WithArguments("FileInfo.Length"));
        }

        [Fact]
        public async Task ShorthandAfterPropertyStep_ReportsOnPropertyType()
        {
            await Verifier.VerifyAsync("UTF2006/NestedPropertyStep.cs",
                Verifier.Diagnostic().WithLocation(18, 47).WithArguments("FileHolder.File"),
                Verifier.Diagnostic().WithLocation(18, 69).WithArguments("FileInfo.Length"));
        }

        [Fact]
        public async Task ShorthandAfterResolvedConstraintAnd_ReportsOnActualType()
        {
            await Verifier.VerifyAsync("UTF2006/ResolvedConstraintThenAnd.cs",
                Verifier.Diagnostic().WithLocation(13, 52).WithArguments("FileInfo.Directory"),
                Verifier.Diagnostic().WithLocation(13, 97).WithArguments("FileInfo.Length"));
        }

        [Theory]
        [InlineData("ReadPropertyDirectlyGood")]
        [InlineData("ExceptionMessageOverride")]
        [InlineData("InnerExceptionOnException")]
        [InlineData("LengthOnString")]
        [InlineData("LengthOnArray")]
        [InlineData("CountOnList")]
        [InlineData("CountOnHashSet")]
        [InlineData("LengthOnMemoryStream")]
        [InlineData("ObjectActual")]
        [InlineData("ConstraintInVariable")]
        public async Task ShorthandOnKeptOrUnresolvedProperty_NoDiagnostic(string fixture)
        {
            await Verifier.VerifyAsync($"UTF2006/{fixture}.cs");
        }

        [Theory]
        [InlineData("EditorOnlyOneEditor")]
        [InlineData("EditorOnlyAllEditors")]
        [InlineData("EditorOnlyArrayArgument")]
        [InlineData("EditorOnlyNamedInclude")]
        [InlineData("EditorOnlyWithExclude")]
        [InlineData("EditorOnlyAmongMultiple")]
        [InlineData("EditorOnlyInLambda")]
        [InlineData("EditorOnlyInLocalFunction")]
        [InlineData("EditorOnlyPropertyInVariable")]
        [InlineData("EditorOnlyFixture")]
        [InlineData("EditorOnlyAssembly")]
        public async Task ConstraintInEditorOnlyScope_NoDiagnostic(string fixture)
        {
            await Verifier.VerifyAsync($"UTF2006/{fixture}.cs");
        }

        [Theory]
        [InlineData("UnityPlatformNoArguments", 15, 52)]
        [InlineData("UnityPlatformExcludeOnly", 15, 52)]
        [InlineData("UnityPlatformIncludesPlayer", 15, 52)]
        [InlineData("UnityPlatformOnSetUp", 15, 52)]
        [InlineData("UnityPlatformOnOuterClass", 17, 56)]
        public async Task ConstraintOutsideEditorOnlyScope_Reports(string fixture, int line, int column)
        {
            await Verifier.VerifyAsync($"UTF2006/{fixture}.cs",
                Verifier.Diagnostic().WithLocation(line, column).WithArguments("FileInfo.Length"));
        }

        [Theory]
        [InlineData("/Assets/Tests/Editor/HasLengthOnFileInfo.cs")]
        [InlineData("/Packages/com.example.foo/Editor/HasLengthOnFileInfo.cs")]
        [InlineData(@"C:\Library\PackageCache\com.example.foo@1.0.0\Tests\Editor\HasLengthOnFileInfo.cs")]
        public async Task ConstraintUnderEditorDirectory_NoDiagnostic(string path)
        {
            await VerifyAtPathAsync("HasLengthOnFileInfo", path);
        }

        [Theory]
        [InlineData("/Assets/Tests/Runtime/HasLengthOnFileInfo.cs")]
        [InlineData("/Assets/EditorTests/HasLengthOnFileInfo.cs")]
        [InlineData("/Assets/editor/HasLengthOnFileInfo.cs")]
        public async Task ConstraintOutsideEditorDirectory_Reports(string path)
        {
            await VerifyAtPathAsync("HasLengthOnFileInfo", path,
                Verifier.Diagnostic().WithLocation(path, 13, 52).WithArguments("FileInfo.Length"));
        }

        /// <summary>
        /// The harness adds the fixture as "/0/Test0.cs", which is under no Editor directory, so the path cases add it
        /// under an explicit path instead.
        /// </summary>
        private static Task VerifyAtPathAsync(string fixture, string path, params DiagnosticResult[] expected)
        {
            var test = new Verifier.Test();
            test.TestState.Sources.Add((path,
                File.ReadAllText(Path.Combine(TestDataFiles.Root, "UTF2006", fixture + ".cs"))));
            foreach (var dummy in TestDataFiles.Dummies)
            {
                test.TestState.Sources.Add(dummy);
            }

            test.ExpectedDiagnostics.AddRange(expected);
            return test.RunAsync();
        }
    }
}
