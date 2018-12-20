using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;

namespace FL.LigArchivar.Core.Tests
{
    internal static class FileSystemForTesting
    {
        private static readonly MockFileSystem _fileSystemForTesting = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                { @"Digitalfoto\2018\A-Albverein\A_2018-05-01_Maiwanderung\A_2018-05-01_001.jpg", new MockFileData(string.Empty) },
                { @"Digitalfoto\2018\A-Albverein\A_2018-05-01_Maiwanderung\A_2018-05-01_002.jpg", new MockFileData(string.Empty) },
                { @"Digitalfoto\2018\A-Albverein\A_2018-05-01_Maiwanderung\A_2018-05-01_003.jpg", new MockFileData(string.Empty) },
                { @"Digitalfoto\2018\A-Albverein\A_2018-05-01_Maiwanderung\A_2018-05-01_004.jpg", new MockFileData(string.Empty) },
                { @"Digitalfoto\2018\A-Albverein\A_2018-05-01_Maiwanderung\A_2018-05-01_005.jpg", new MockFileData(string.Empty) }
            },
            @"\\sepp\archiv");

        public static IFileSystem Instance => _fileSystemForTesting;
    }
}
