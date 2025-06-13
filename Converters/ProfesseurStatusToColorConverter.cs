using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace centre_soutien.Converters;

public class ProfesseurStatusToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool estArchive)
        {
            if (!estArchive) // Si pas archivé = actif
            {
                // Vert pour actif
                return new LinearGradientBrush(
                    new GradientStopCollection
                    {
                        new GradientStop(Color.FromRgb(72, 187, 120), 0),
                        new GradientStop(Color.FromRgb(56, 161, 105), 1)
                    },
                    new Point(0, 0), new Point(1, 1));
            }
            else // Si archivé
            {
                // Rouge pour archivé
                return new LinearGradientBrush(
                    new GradientStopCollection
                    {
                        new GradientStop(Color.FromRgb(245, 101, 101), 0),
                        new GradientStop(Color.FromRgb(229, 62, 62), 1)
                    },
                    new Point(0, 0), new Point(1, 1));
            }
        }
        return Brushes.Gray;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}