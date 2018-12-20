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
            var created = ArchiveRoot.TryCreate(@"C:\archiv", out _root);
            Assert.IsTrue(created);
        }

        [Test]
        public void GetYear()
        {
            var child = _root.GetChild(@"Digitalfoto\2018\A-Albverein");

            var actualYear = child.GetYear();

            Assert.AreEqual("2018", actualYear);
        }

        [Test]
        public void GetClubChar()
        {
            var child = _root.GetChild(@"Digitalfoto\2018\A-Albverein");

            var actualYear = child.GetClubChar();

            Assert.AreEqual("A", actualYear);
        }
    }
}
