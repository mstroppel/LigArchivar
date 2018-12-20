using FL.LigArchivar.Core.Data;
using NUnit.Framework;

namespace FL.LigArchivar.Core.Tests.Data
{
    [TestFixture]
    public class FileSystemItemExtensionsTests
    {
        private ArchiveRoot _root;

        [SetUp]
        public void SetUp()
        {
            FileSystemProvider.Instance = FileSystemForTesting.Instance;
            var created = ArchiveRoot.TryCreate(@"\\sepp\archiv", out _root);
            Assert.IsTrue(created);
        }

        [Test]
        public void GetYear()
        {
            var child = _root.GetChild(@"Digitalfoto\2018\A-Albverein\A_2018-05-01_Maiwanderung");

            var actualYear = child.GetYear();

            Assert.AreEqual("2018", actualYear);
        }
    }
}
