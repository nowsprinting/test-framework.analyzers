using System.Threading.Tasks;
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
    }
}
