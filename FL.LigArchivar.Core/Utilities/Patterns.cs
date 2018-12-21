namespace FL.LigArchivar.Core.Utilities
{
    public sealed class Patterns
    {
        private const string ClubPart = @"([A-Z])";
        private const string YearPart = @"([12][0-9]{3})";
        private const string MonthPart = @"(0[1-9]|1[0-2])";
        private const string DayPart = @"(0[0-9]|[12][0-9]|3[01])";

        private const string EventNamePart = @"([a-zA-Z0-9\-_]{1,200})";
        private const string NumberPart = @"[0-9]{3}";
        private const string PropertyPart = @"([a-zA-Z0-9\-_]{1,200})";

        public static readonly string EventDirectory = $"{ClubPart}_{YearPart}-{MonthPart}-{DayPart}_{EventNamePart}";
        public static readonly string DataFile = $"{ClubPart}_{YearPart}-{MonthPart}-{DayPart}_{NumberPart}(_{PropertyPart})?";
    }
}
