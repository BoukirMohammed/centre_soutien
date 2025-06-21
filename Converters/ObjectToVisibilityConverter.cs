using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace centre_soutien.Views
{
    /// <summary>
    /// Convertisseur qui affiche un élément si l'objet n'est pas null, le cache sinon
    /// </summary>
    public class ObjectToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}