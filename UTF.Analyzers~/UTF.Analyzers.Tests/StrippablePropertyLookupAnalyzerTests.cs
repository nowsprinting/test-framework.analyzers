using System.Threading.Tasks;
using Xunit;
using Verifier = UTF.Analyzers.Tests.TestDataVerifier<UTF.Analyzers.StrippablePropertyLookupAnalyzer>;

namespace UTF.Analyzers.Tests
{
    public class StrippablePropertyLookupAnalyzerTests
    {
        [Theory]
        [InlineData("HasLengthOnFileInfo", 13, 52, "FileInfo.Length")]
        [InlineData("ThrowsTypeOfWithProperty", 19, 61, "ArgumentException.ParamName")]
        [InlineData("ThrowsArgumentExceptionWithProperty", 18, 74, "ArgumentException.ParamName")]
        [InlineData("HasPropertyExists", 13, 71, "ArgumentException.ParamName")]
        [InlineData("HasNoProperty", 13, 55, "FileInfo.Length")]
        [InlineData("NameofPropertyName", 13, 52, "FileInfo.Length")]
        [InlineData("ActualValueDelegate", 13, 58, "FileInfo.Length")]
        [InlineData("AssumeThat", 13, 52, "FileInfo.Length")]
        [InlineData("IsTypeOfAndProperty", 14, 59, "FileInfo.Length")]
        [InlineData("UserTypeProperty", 18, 47, "ScoreBoard.Score")]
        public async Task PropertyOfTargetType_ReportsAtPropertyName(string fixture, int line, int column, string property)
        {
            await Verifier.VerifyAsync($"UTF2006/{fixture}.cs",
                Verifier.Diagnostic().WithLocation(line, column).WithArguments(property));
        }

        [Theory]
        [InlineData("HasSomePropertyOnArray", 14, 41)]
        [InlineData("HasAllLengthOnList", 14, 40)]
        [InlineData("HasExactlyProperty", 14, 47)]
        [InlineData("OrderedBy", 14, 43)]
        [InlineData("ListMapProperty", 14, 41)]
        public async Task PropertyOfElementType_ReportsAtPropertyName(string fixture, int line, int column)
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
        public async Task NestedPropertyStep_ReportsInnerPropertyOnOuterPropertyType()
        {
            await Verifier.VerifyAsync("UTF2006/NestedPropertyStep.cs",
                Verifier.Diagnostic().WithLocation(13, 52).WithArguments("FileInfo.Directory"),
                Verifier.Diagnostic().WithLocation(13, 79).WithArguments("DirectoryInfo.Parent"));
        }

        [Fact]
        public async Task ResolvedConstraintThenAnd_ReportsBothPropertiesOnActualType()
        {
            await Verifier.VerifyAsync("UTF2006/ResolvedConstraintThenAnd.cs",
                Verifier.Diagnostic().WithLocation(13, 52).WithArguments("FileInfo.Length"),
                Verifier.Diagnostic().WithLocation(13, 90).WithArguments("FileInfo.IsReadOnly"));
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
        [InlineData("TypeParameterActual")]
        [InlineData("SystemTypeActual")]
        [InlineData("NonConstantName")]
        [InlineData("PropertyNotFoundOnStaticType")]
        [InlineData("ConstraintInVariable")]
        public async Task NoStrippableProperty_NoDiagnostic(string fixture)
        {
            await Verifier.VerifyAsync($"UTF2006/{fixture}.cs");
        }
    }
}
