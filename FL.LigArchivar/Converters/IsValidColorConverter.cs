using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Caliburn.Micro;

namespace FL.LigArchivar.Converters
{
    public class IsValidColorConverter : IValueConverter
    {
        private static readonly ILog Log = LogManager.GetLog(typeof(IsValidColorConverter));
        private static readonly SolidColorBrush ValidBrush = new SolidColorBrush(Colors.Black);
        private static readonly SolidColorBrush InvalidBrush = new SolidColorBrush(Colors.Red);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isValid = value as bool?;

            if (!isValid.HasValue)
            {
                Log.Warn("Failed to parse expected value as boolean: '" + value);
                return InvalidBrush;
            }

            if (isValid.Value)
                return ValidBrush;
            else
                return InvalidBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
