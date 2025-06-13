using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace centre_soutien.Converters
{
    // ==========================================
    // CONVERTISSEURS POUR LE PLANNING
    // ==========================================

    /// <summary>
    /// Convertit bool en Visibility inverse (true = Collapsed, false = Visible)
    /// </summary>
    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool bValue)
            {
                return bValue ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Convertit les dates string en format d'affichage dd/MM/yyyy
    /// </summary>
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

    /// <summary>
    /// Convertit les heures string en format d'affichage HH:mm
    /// </summary>
    public class TimeStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string timeString && !string.IsNullOrWhiteSpace(timeString))
            {
                // Essayer de parser comme TimeSpan ou comme heure
                if (TimeSpan.TryParse(timeString, out TimeSpan time))
                {
                    return time.ToString(@"hh\:mm");
                }
                
                // Essayer de parser comme DateTime
                if (DateTime.TryParse(timeString, out DateTime dateTime))
                {
                    return dateTime.ToString("HH:mm");
                }
                
                // Si c'est déjà au bon format, le retourner
                if (timeString.Contains(":") && timeString.Length >= 4)
                {
                    return timeString;
                }
            }
            return "-";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Convertit le statut bool en couleur (actif = vert, inactif = rouge)
    /// </summary>
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

    /// <summary>
    /// Convertit le statut bool en texte (actif = ACTIVE, inactif = INACTIVE)
    /// </summary>
    public class StatusToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isActive)
            {
                return isActive ? "ACTIF" : "INACTIF";
            }
            return "INCONNU";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Convertit le statut bool en icône pour bouton toggle (actif = pause, inactif = play)
    /// </summary>
    public class ActiveToggleTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isActive)
            {
                return isActive ? "⏸" : "▶"; // Pause si actif, Play si inactif
            }
            return "?";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Convertit le statut bool en tooltip pour bouton toggle
    /// </summary>
    public class ActiveToggleTooltipConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isActive)
            {
                return isActive ? "Désactiver ce créneau" : "Activer ce créneau";
            }
            return "Changer le statut";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Convertit le statut bool en couleur pour bouton toggle (actif = orange, inactif = vert)
    /// </summary>
    public class ActiveToggleColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isActive)
            {
                if (isActive)
                {
                    // Orange pour désactiver
                    return new LinearGradientBrush(
                        new GradientStopCollection
                        {
                            new GradientStop(Color.FromRgb(237, 137, 54), 0),
                            new GradientStop(Color.FromRgb(221, 107, 32), 1)
                        },
                        new Point(0, 0), new Point(1, 1));
                }
                else
                {
                    // Vert pour activer
                    return new LinearGradientBrush(
                        new GradientStopCollection
                        {
                            new GradientStop(Color.FromRgb(72, 187, 120), 0),
                            new GradientStop(Color.FromRgb(56, 161, 105), 1)
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

    /// <summary>
    /// Affiche/masque selon qu'une chaîne est vide ou non
    /// </summary>
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
    
}