using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using Caliburn.Micro;
using FL.LigArchivar.Core.Utilities;

namespace FL.LigArchivar.Core.Data
{
    /// <summary>
    /// Stores all application settings. Is used to read from and write to an XML file.
    /// </summary>
    public class AppSettings : PropertyChangedBase
    {
        /// <summary>
        /// The default settings.
        /// </summary>
        public static readonly AppSettings Default = CreateDefault();

        /// <summary>
        /// Raises the <see cref="INotifyPropertyChanged.PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        public override void NotifyOfPropertyChange([CallerMemberName] string propertyName = null)
        {
            base.NotifyOfPropertyChange(propertyName);
            if (propertyName != "Changed")
            {
                Changed = true;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="AppSettings"/> is changed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if changed; otherwise, <c>false</c>.
        /// </value>
        [XmlIgnore]
        public bool Changed
        {
            get { return _changed; }
            set
            {
                if (_changed != value)
                {
                    _changed = value;
                    NotifyOfPropertyChange(() => Changed);
                }
            }
        }

        private bool _changed;

        /// <summary>
        /// Gets the application settings path where these settings have been loaded from.
        /// </summary>
        /// <value>
        /// Is <c>null</c> if the file has not been loaded from a file.
        /// </value>
        [XmlIgnore]
        public string AppSettingsPath
        {
            get { return _appSettingsPath; }
            private set
            {
                if (_appSettingsPath != value)
                {
                    _appSettingsPath = value;
                    NotifyOfPropertyChange(() => AppSettingsPath);
                }
            }
        }

        private string _appSettingsPath;

        /// <summary>
        /// Gets or sets the root directory to work with.
        /// </summary>
        public string RootDirectory
        {
            get { return _rootDirectory; }
            set
            {
                if (_rootDirectory != value)
                {
                    _rootDirectory = value;
                    NotifyOfPropertyChange(() => RootDirectory);
                }
            }
        }

        private string _rootDirectory;

        /// <summary>
        /// Gets or sets the allowed file extensions.
        /// </summary>
        [XmlArrayItem(ElementName = "Extension")]
#pragma warning disable CA2227 // Collection properties should be read only - simplest solution for XML serialization
        public List<string> FileExtensions
#pragma warning restore CA2227 // Collection properties should be read only
        {
            get { return _fileExtensions; }
            set
            {
                if (_fileExtensions != value)
                {
                    _fileExtensions = value;
                    NotifyOfPropertyChange(() => FileExtensions);
                }
            }
        }

        private List<string> _fileExtensions;

        /// <summary>
        /// Loads the settings from a specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>The read settings.</returns>
        public static AppSettings Load(string path)
        {
            var retVal = XmlSerializerEx.LoadFromFile<AppSettings>(path);
            retVal.AppSettingsPath = path;
            retVal.Changed = false;
            return retVal;
        }

        /// <summary>
        /// Saves this settings to the specified path.
        /// </summary>
        /// <param name="path">The path. If <c>null</c> the path <see cref="AppSettingsPath"/>
        /// will be used.</param>
        /// <exception cref="System.InvalidOperationException">No path provided and application settings not loaded from a file.</exception>
        public void Save(string path = null)
        {
            if (path == null)
                path = AppSettingsPath;
            if (path == null)
                throw new InvalidOperationException("No path provided and application settings not loaded from a file");
            XmlSerializerEx.SaveToFile(path, this);
            Changed = false;
        }

        /// <summary>
        /// Creates the an instance with default values.
        /// </summary>
        /// <returns>The created instance with default data.</returns>
        private static AppSettings CreateDefault()
        {
            var instance = new AppSettings
            {
                RootDirectory = @"\\finsch\Daten\Temp\LigaArchiv\dickdone\archiv",
                FileExtensions = new List<string>(new string[]
                {
                    @".jpg",
                    @".dng",
                    @".mts",
                }),
            };

            return instance;
        }
    }
}
