using FL.LigArchivar.Core.Data;
using NUnit.Framework;

namespace FL.LigArchivar.Core.Tests.Data
{
    [TestFixture]
    public class EventDirectoryTests
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
        public void TryCreate()
        {
            // Arrange
            var uut = _root.GetChild(@"Digitalfoto\2018\A-Albverein\A_2018-05-01_Maiwanderung");

            // Act
            var uutAsEventDirectory = uut as EventDirectory;

            // Assert
            Assert.IsNotNull(uutAsEventDirectory);

            Assert.AreEqual("A", uutAsEventDirectory.ClubChar);
            Assert.AreEqual("2018", uutAsEventDirectory.Year);
            Assert.AreEqual("05", uutAsEventDirectory.Month);
            Assert.AreEqual("01", uutAsEventDirectory.Day);
            Assert.AreEqual("Maiwanderung", uutAsEventDirectory.EventName);
        }
    }
}
