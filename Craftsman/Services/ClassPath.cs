namespace Craftsman.Services;

using System.IO;

public interface IClassPath
{
    string ClassDirectory { get; }
    string ClassName { get; set; }
    string ClassNameWithoutExt { get; }
    string ClassNamespace { get; }
    string FullClassPath { get; }
    string SolutionDirectory { get; set; }
}

public class ClassPath : IClassPath
{
    public ClassPath(string solutionDirectory, string topPath, string className)
    {
        ClassDirectory = Path.Combine(solutionDirectory, topPath);
        FullClassPath = Path.Combine(ClassDirectory, className);
        ClassNamespace = topPath.Replace(Path.DirectorySeparatorChar, '.');
        SolutionDirectory = solutionDirectory;
        ClassName = className;
        
        int lastDot = ClassName.LastIndexOf('.');
        ClassNameWithoutExt = lastDot > 0 ? ClassName.Substring(0, lastDot) : ClassName;
    }

    /// <summary>
    /// This is the full path to the class, *without* the filename. For filename, use FullClassPath.
    /// </summary>
    public string ClassDirectory { get; private set; }

    /// <summary>
    /// This is the full path to the class, including the filename (e.g. C:\repo\Pets.cs)
    /// </summary>
    public string FullClassPath { get; private set; }

    public string ClassNameWithoutExt { get; private set; }

    /// <summary>
    /// This is the converted namespace from the top path
    /// </summary>
    public string ClassNamespace { get; private set; }

    /// <summary>
    /// This is the solution directory mirrored back
    /// </summary>
    public string SolutionDirectory { get; set; }

    /// <summary>
    /// This is the classname mirrored back
    /// </summary>
    public string ClassName { get; set; }
}
