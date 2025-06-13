using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace centre_soutien.Views
{
    // Convertisseur pour la couleur du statut (actif/inactif)
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isActive)
            {
                if (isActive)
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
                else
                {
                    // Rouge pour inactif
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

    // Convertisseur pour le texte du statut
    public class StatusToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isActive)
            {
                return isActive ? "ACTIVE" : "INACTIVE";
            }
            return "INCONNU";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Convertisseur pour afficher/masquer selon qu'une chaîne est vide ou non
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                return string.IsNullOrWhiteSpace(str) ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Convertisseur pour formater les dates string en format d/M/yyyy
    public class StringDateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string dateString && !string.IsNullOrWhiteSpace(dateString))
            {
                if (DateTime.TryParse(dateString, out DateTime date))
                {
                    return date.ToString("dd/MM/yyyy");
                }
            }
            return "-";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class SalleStatusToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool estArchivee)
        {
            if (!estArchivee) // Si pas archivée = active
            {
                // Vert pour active
                return new LinearGradientBrush(
                    new GradientStopCollection
                    {
                        new GradientStop(Color.FromRgb(72, 187, 120), 0),
                        new GradientStop(Color.FromRgb(56, 161, 105), 1)
                    },
                    new Point(0, 0), new Point(1, 1));
            }
            else // Si archivée
            {
                // Rouge pour archivée
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

// Convertisseur pour le texte du statut des salles
public class SalleStatusToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool estArchivee)
        {
            return estArchivee ? "ARCHIVÉE" : "ACTIVE";
        }
        return "INCONNU";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

// Convertisseur pour le texte du bouton d'archivage
public class ArchiveToggleTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool estArchivee)
        {
            return estArchivee ? "📂" : "🗃️";
        }
        return "🗃️";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

// Convertisseur pour le tooltip du bouton d'archivage
public class ArchiveToggleTooltipConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool estArchivee)
        {
            return estArchivee ? "Désarchiver la salle" : "Archiver la salle";
        }
        return "Archiver la salle";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
}