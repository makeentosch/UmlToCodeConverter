using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Presentation.Wpf.Converters;

public class StatusColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var status = value as string;

        if (string.IsNullOrEmpty(status))
            return Brushes.Black;

        if (status.Contains("Error", StringComparison.OrdinalIgnoreCase))
            return Brushes.Red;

        if (status.Contains("Success", StringComparison.OrdinalIgnoreCase))
            return Brushes.DarkGreen;

        return Brushes.Black;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}