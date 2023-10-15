using System.IO;

namespace Tql.Setup.App.Support;

internal static class DirectoryEx
{
    public static void ForceDeleteRecursive(string path)
    {
        foreach (var directory in Directory.GetDirectories(path))
        {
            ForceDeleteRecursive(directory);
        }

        foreach (var fileName in Directory.GetFiles(path))
        {
            new FileInfo(fileName).IsReadOnly = false;
            File.Delete(fileName);
        }

        new FileInfo(path).IsReadOnly = false;
        Directory.Delete(path);
    }
}
