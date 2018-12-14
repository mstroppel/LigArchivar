namespace FL.LigArchivar.Core.Data
{
    public interface IFileSystemItem
    {
        string Name { get; }

        bool Valid { get; }
    }
}
