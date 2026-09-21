namespace Integration.Core;

/// <summary>
/// Locates the shared local "data" folder that stands in for AWS S3 during Week 1.
/// Both ERP.Simulator (writer) and Integration.Worker (reader) resolve the same
/// path by walking up from their own build output to the solution root, so the
/// folder is found the same way regardless of the working directory the process
/// was launched from.
/// </summary>
public static class PipelineFolders
{
    public static string Incoming => GetOrCreate("incoming");

    public static string Processed => GetOrCreate("processed");

    public static string Output => GetOrCreate("output");

    private static string GetOrCreate(string subfolder)
    {
        var path = Path.Combine(SolutionRoot, "data", subfolder);
        Directory.CreateDirectory(path);
        return path;
    }

    private static string SolutionRoot
    {
        get
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir is not null && dir.GetFiles("*.slnx").Length == 0)
            {
                dir = dir.Parent;
            }

            return dir?.FullName ?? Directory.GetCurrentDirectory();
        }
    }
}
