namespace FL.LigArchivar.Core.Data
{
    public class InvalidFileSystemItem : IFileSystemItem
    {
        public InvalidFileSystemItem(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public bool Valid => false;
    }
}
