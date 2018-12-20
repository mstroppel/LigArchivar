using System.IO.Abstractions;
using System.Text.RegularExpressions;

namespace FL.LigArchivar.Core.Data
{
    public class EventDirectory : FileSystemItemBase
    {
        private const string RegExClubPart = @"([A-Z])";
        private const string RegExYearPart = @"([12][0-9]{3})";
        private const string RegExMonthPart = @"(0[1-9]|1[0-2])";
        private const string RegExDayPart = @"(0[0-9]|[12][0-9]|3[01])";
        private const string RegExEventNamePart = @"([a-zA-Z0-9\-_]{1,200})";

        public EventDirectory(DirectoryInfoBase directory, IFileSystemItem parent, bool isValid, string clubChar, string year, string month, string day, string eventName)
            : base(directory, directory.Name, parent, isValid)
        {
            ClubChar = clubChar;
            Year = year;
            Month = month;
            Day = day;
            EventName = eventName;

            FilePrefix = clubChar + "_" + year + "-" + month + "-" + day + "_";
        }

        public string ClubChar { get; }

        public string Year { get; }

        public string Month { get; }

        public string Day { get; }

        public string EventName { get; }

        public string FilePrefix { get; }

        public static bool TryCreate(DirectoryInfoBase eventDirectory, IFileSystemItem parent, out IFileSystemItem directory)
        {
            directory = null;

            var name = eventDirectory.Name;

            var expectedClubChar = parent.GetClubChar();
            var expectedYear = parent.GetYear();

            var regexString = $"{RegExClubPart}_{RegExYearPart}-{RegExMonthPart}-{RegExDayPart}_{RegExEventNamePart}";
            var regex = new Regex(regexString);

            var match = regex.Match(name);

            if (!match.Success)
                return false;

            if (match.Groups.Count != 6)
                return false;

            var actualClubChar = match.Groups[1].Value;
            var actualYear = match.Groups[2].Value;
            var actualMonth = match.Groups[3].Value;
            var actualDay = match.Groups[4].Value;
            var actualEventName = match.Groups[5].Value;

            var doesClubCharMatch = expectedClubChar == actualClubChar;
            var doesYearMatch = expectedYear == actualYear;
            var isValid = doesClubCharMatch && doesYearMatch;

            directory = new EventDirectory(eventDirectory, parent, isValid, actualClubChar, actualYear, actualMonth, actualDay, actualEventName);
            return true;
        }
    }
}
