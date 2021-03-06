﻿using System.Collections.Immutable;

namespace FL.LigArchivar.Core.Data
{
    public interface IFileSystemItem
    {
        string Name { get; }

        bool IsValid { get; }

        IFileSystemItem Parent { get; }
    }
}
