namespace Craftsman.Models.Interfaces
{
    public interface IClassPath
    {
        string ClassDirectory { get; }
        string ClassName { get; set; }
        string ClassNamespace { get; }
        string FullClassPath { get; }
        string SolutionDirectory { get; set; }
    }
}