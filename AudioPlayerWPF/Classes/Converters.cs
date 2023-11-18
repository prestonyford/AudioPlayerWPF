using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AudioPlayerWPF.Classes {
    public class SecondsToTimeSpanConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is double seconds) {
                return TimeSpan.FromSeconds(seconds);
            }
            return TimeSpan.Zero; // default value if value cannot be converted
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
    public class MillisecondsToStringConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is double milliseconds) {
                TimeSpan ts = TimeSpan.FromMilliseconds(milliseconds);
                return $"{ts.Minutes.ToString().PadLeft(2, '0')}:{ts.Seconds.ToString().PadLeft(2, '0')}.{ts.Milliseconds.ToString().PadLeft(3, '0')}";
            }
            return "00:00.000"; // default value if value cannot be converted
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    public class DoubleToMarginLeftConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is double offset) {
                if (parameter is string par) {
                    offset += double.Parse(par);
                }
                return new Thickness(offset, 0, 0, 0);
            }

            return new Thickness(0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }


}
