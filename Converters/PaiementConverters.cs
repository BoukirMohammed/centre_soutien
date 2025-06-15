using System;
using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using centre_soutien.ViewModels;
using System.Collections.Generic; // AJOUTÉ
using System.Linq; // AJOUTÉ

namespace centre_soutien.Converters
{
    /// <summary>
    /// Convertit un booléen en couleur pour le statut de paiement
    /// </summary>
    public class BooleanToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isAJour)
            {
                return new SolidColorBrush(isAJour ? Colors.Green : Colors.Red);
            }
            
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Convertit un booléen en texte de statut
    /// </summary>
    public class BooleanToStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isAJour)
            {
                return isAJour ? "À jour" : "En retard";
            }
            
            return "Inconnu";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Version améliorée avec couleurs personnalisées pour les paiements
    /// </summary>
    public class PaiementStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isAJour)
            {
                return new SolidColorBrush(isAJour ? 
                    Color.FromRgb(72, 187, 120) :   // Vert plus doux (#48bb78)
                    Color.FromRgb(245, 101, 101));  // Rouge plus doux (#f56565)
            }
            
            return new SolidColorBrush(Color.FromRgb(113, 128, 150)); // Gris (#718096)
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// Convertit un montant en couleur selon sa valeur (pour l'affichage visuel)
    /// </summary>

    public class AmountToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double amount)
            {
                if (amount == 0)
                    return new SolidColorBrush(Colors.Gray);
                if (amount > 0)
                    return new SolidColorBrush(Colors.Green);
                return new SolidColorBrush(Colors.Red);
            }
        
            return new SolidColorBrush(Colors.Black);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Convertit le nombre d'échéances sélectionnées en visibilité du bouton de paiement
    /// </summary>
    public class CountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count)
            {
                return count > 0 ? Visibility.Visible : Visibility.Collapsed;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Convertit une collection d'échéances en nombre d'échéances sélectionnées
    /// </summary>
    public class CountSelectedEcheancesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IEnumerable<EcheanceDisplayItem> echeances)
            {
                return echeances.Count(e => e.EstSelectionne);
            }
            
            if (value is IEnumerable enumerable)
            {
                var count = 0;
                foreach (var item in enumerable)
                {
                    if (item is EcheanceDisplayItem echeance && echeance.EstSelectionne)
                    {
                        count++;
                    }
                }
                return count;
            }
            
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Convertit une collection d'échéances en somme des montants des échéances sélectionnées
    /// </summary>
    public class SumSelectedEcheancesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IEnumerable<EcheanceDisplayItem> echeances)
            {
                return echeances.Where(e => e.EstSelectionne).Sum(e => e.MontantAPayer);
            }

            if (value is IEnumerable enumerable)
            {
                double sum = 0;
                foreach (var item in enumerable)
                {
                    if (item is EcheanceDisplayItem echeance && echeance.EstSelectionne)
                    {
                        sum += echeance.MontantAPayer;
                    }
                }

                return sum;
            }

            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }

    /// <summary>
    /// Version alternative qui retourne un texte formaté directement
    /// </summary>
    public class SelectedEcheancesTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IEnumerable<EcheanceDisplayItem> echeances)
            {
                var selectedItems = echeances.Where(e => e.EstSelectionne).ToList();
                var count = selectedItems.Count;
                var sum = selectedItems.Sum(e => e.MontantAPayer);
                
                return $"{count} échéance(s) sélectionnée(s) - Total: {sum:C}";
            }
            
            return "Aucune échéance sélectionnée";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Convertisseur pour formater le montant avec la devise marocaine
    /// </summary>
    public class MontantToMarocainCurrencyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double montant)
            {
                return montant.ToString("C", new CultureInfo("fr-MA"));
            }
            
            if (value is decimal montantDecimal)
            {
                return montantDecimal.ToString("C", new CultureInfo("fr-MA"));
            }
            
            return "0,00 DH";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// Convertit une collection null/vide en Visibility
    /// </summary>
    public class CollectionToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is System.Collections.ICollection collection)
            {
                bool hasItems = collection.Count > 0;
                bool isInverse = parameter?.ToString() == "Inverse";
                
                if (isInverse)
                    return hasItems ? Visibility.Collapsed : Visibility.Visible;
                else
                    return hasItems ? Visibility.Visible : Visibility.Collapsed;
            }
            
            // Si ce n'est pas une collection, traiter comme null
            bool isNull = value == null;
            bool isInverseParam = parameter?.ToString() == "Inverse";
            
            if (isInverseParam)
                return isNull ? Visibility.Visible : Visibility.Collapsed;
            else
                return isNull ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// Convertit null en Visibility.Visible et non-null en Visibility.Collapsed
    /// </summary>
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isInverse = parameter?.ToString() == "Inverse";
            bool isNull = value == null;
            
            if (isInverse)
                return isNull ? Visibility.Collapsed : Visibility.Visible;
            else
                return isNull ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
