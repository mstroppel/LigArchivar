using System.Globalization;
using System.IO;
using System.Security;
using System.Text;
using System.Xml.Serialization;

namespace FL.LigArchivar.Core.Utilities
{
    /// <summary>
    /// Helper to serialize and deserialize objects to and from XML files.
    /// </summary>
    public static class XmlSerializerEx
    {
        private const string _fileNotFoundMessage = "Datei nicht gefunden.";
        private const string _notAccessMessage = "Zugriff auf Datei '{0}'nicht erlaubt/möglich.";
        private const string _cannotReadMessage = "Kann Datei '{0}' nicht lesen.";
        private const string _cannotSaveMessage = "Kann Datei '{0}' nicht speichern.";

        /// <summary>
        /// Load the data from an XML file to an object of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the object to read.</typeparam>
        /// <param name="filePath">The file path.</param>
        /// <returns>The object that was read.</returns>
        /// <exception cref="FileNotFoundException">If file was not found.</exception>
        /// <exception cref="SecurityException">If there are not enough rights
        /// to open the file.</exception>
        /// <exception cref="System.Exception">Another exception. Read the message and the <see cref="System.Exception.InnerException"/>
        /// property for more details.</exception>
        public static T LoadFromFile<T>(string filePath)
            where T : class
        {
            T readObject = null;
            try
            {
                // Try to open existing file with default deserialization.
                using (Stream stream = FileSystemProvider.Instance.FileStream.Create(filePath, FileMode.Open))
                using (StreamReader sr = new StreamReader(stream, Encoding.UTF8, true))
                {
                    var serializer = new XmlSerializer(typeof(T));
                    readObject = serializer.Deserialize(sr) as T;
                }
            }
            catch (FileNotFoundException e)
            {
                throw new FileNotFoundException(_fileNotFoundMessage, filePath, e);
            }
            catch
            {
                // On all other exceptions we will try the tolerant XML reader.
            }

            if (readObject != null)
            {
                return readObject;
            }

            // Try to open existing file with tolerant deserialization.
            try
            {
                using (Stream stream = FileSystemProvider.Instance.FileStream.Create(filePath, FileMode.Open))
                using (StreamReader sr = new StreamReader(stream, Encoding.UTF8, true))
                {
                    var serializer = new XmlSerializer(typeof(T));
                    return serializer.Deserialize(sr) as T;
                }
            }
            catch (SecurityException e)
            {
                throw new SecurityException(string.Format(CultureInfo.InvariantCulture, _notAccessMessage, filePath), e);
            }
            catch (System.Exception ex)
            {
                throw new System.Exception(string.Format(CultureInfo.InvariantCulture, _cannotReadMessage, filePath), ex);
            }
        }

        /// <summary>
        /// Serializes the passed object to an XML file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="data">The data to store.</param>
        /// <exception cref="FileNotFoundException">If file was not found.</exception>
        /// <exception cref="SecurityException">If there are not enough rights
        /// to open the file.</exception>
        /// <exception cref="System.Exception">Another exception. Read the message and the <see cref="System.Exception.InnerException"/>
        /// property for more details.</exception>
        public static void SaveToFile(string filePath, object data)
        {
            try
            {
                using (Stream stream = FileSystemProvider.Instance.FileStream.Create(filePath, FileMode.Create))
                using (StreamWriter sw = new StreamWriter(filePath, false, Encoding.UTF8))
                {
                    var serializer = new XmlSerializer(data.GetType());
                    serializer.Serialize(sw, data);
                }
            }
            catch (FileNotFoundException e)
            {
                throw new FileNotFoundException(_fileNotFoundMessage, filePath, e);
            }
            catch (SecurityException e)
            {
                throw new SecurityException(string.Format(CultureInfo.InvariantCulture, _notAccessMessage, filePath), e);
            }
            catch (System.Exception e)
            {
                throw new System.Exception(string.Format(CultureInfo.InvariantCulture, _cannotSaveMessage, filePath), e);
            }
        }
    }
}
