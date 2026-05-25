using Microsoft.UI.Xaml.Data;

namespace VoiceInk.Windows.App;

public sealed class WaveformPeakHeightConverter : IValueConverter
{
    private const double MinHeight = 4;
    private const double MaxHeight = 40;

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var peak = value is double doubleValue ? doubleValue : 0;
        peak = Math.Clamp(peak, 0, 1);
        return MinHeight + (MaxHeight - MinHeight) * peak;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotSupportedException();
}
