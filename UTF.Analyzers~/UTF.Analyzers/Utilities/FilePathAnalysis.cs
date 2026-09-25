namespace UTF.Analyzers.Utilities;

internal static class FilePathAnalysis
{
    /// <param name="filePath">A syntax tree's file path, with either separator</param>
    /// <param name="directory">A directory segment enclosed in slashes, e.g. "/Tests/", matched case-sensitively</param>
    public static bool ContainsDirectory(string filePath, string directory)
    {
        // Normalized by hand rather than through Path: the analyzer runs on macOS against paths that Unity on Windows
        // wrote with backslashes, and Path treats those as part of the file name there.
        return filePath.Replace('\\', '/').Contains(directory);
    }
}
