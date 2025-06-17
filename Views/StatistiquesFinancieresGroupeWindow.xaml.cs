using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using centre_soutien.ViewModels;

namespace centre_soutien.Views
{
    /// <summary>
    /// Fenêtre des statistiques financières pour un groupe
    /// </summary>
    public partial class StatistiquesFinancieresGroupeWindow : Window
    {
        public StatistiquesFinancieresGroupeWindow(int groupeId)
        {
            InitializeComponent();
            DataContext = new StatistiquesFinancieresGroupeViewModel(groupeId);
            
            // Configurer la fenêtre
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ShowInTaskbar = false;
        }

        /// <summary>
        /// Gestionnaire pour le bouton Fermer
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        /// <summary>
        /// Gestionnaire pour fermer la fenêtre avec Escape
        /// </summary>
        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                Close();
                e.Handled = true;
            }
            base.OnKeyDown(e);
        }
    }

    /// <summary>
    /// Convertisseur pour calculer le pourcentage de collecte
    /// </summary>
    public class MathConverter : IMultiValueConverter
    {
        public static readonly MathConverter Instance = new MathConverter();

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values?.Length >= 2 && 
                values[0] is double collecte && 
                values[1] is double attendu && 
                attendu > 0)
            {
                return (collecte / attendu) * 100;
            }
            return 0.0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Convertisseur pour calculer la rémunération du professeur
    /// </summary>
    public class ProfessorRemunerationConverter : IMultiValueConverter
    {
        public static readonly ProfessorRemunerationConverter Instance = new ProfessorRemunerationConverter();

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values?.Length >= 2 && 
                values[0] is double montantTotal && 
                values[1] is double pourcentage)
            {
                return montantTotal * (pourcentage / 100.0);
            }
            return 0.0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Convertisseur pour calculer le profit du centre
    /// </summary>
    public class CenterProfitConverter : IMultiValueConverter
    {
        public static readonly CenterProfitConverter Instance = new CenterProfitConverter();

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values?.Length >= 2 && 
                values[0] is double montantTotal && 
                values[1] is double pourcentage)
            {
                var remunerationProf = montantTotal * (pourcentage / 100.0);
                return montantTotal - remunerationProf;
            }
            return 0.0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Convertisseur pour formater les devises en MAD
    /// </summary>
    public class CurrencyConverter : IValueConverter
    {
        public static readonly CurrencyConverter Instance = new CurrencyConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double amount)
            {
                return amount.ToString("C", new CultureInfo("fr-MA"));
            }
            return "0,00 MAD";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Convertisseur pour les couleurs basées sur les performances
    /// </summary>
    public class PerformanceColorConverter : IValueConverter
    {
        public static readonly PerformanceColorConverter Instance = new PerformanceColorConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double percentage)
            {
                if (percentage >= 90)
                    return "#38a169"; // Vert
                else if (percentage >= 70)
                    return "#dd6b20"; // Orange
                else if (percentage >= 50)
                    return "#d69e2e"; // Jaune
                else
                    return "#e53e3e"; // Rouge
            }
            return "#718096"; // Gris par défaut
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Convertisseur pour les icônes de performance
    /// </summary>
    public class PerformanceIconConverter : IValueConverter
    {
        public static readonly PerformanceIconConverter Instance = new PerformanceIconConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double percentage)
            {
                if (percentage >= 90)
                    return "🎯"; // Excellent
                else if (percentage >= 70)
                    return "👍"; // Bien
                else if (percentage >= 50)
                    return "⚠️"; // Moyen
                else
                    return "🚨"; // Problème
            }
            return "❓"; // Inconnu
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Convertisseur pour les messages de performance
    /// </summary>
    public class PerformanceMessageConverter : IValueConverter
    {
        public static readonly PerformanceMessageConverter Instance = new PerformanceMessageConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double percentage)
            {
                if (percentage >= 95)
                    return "EXCELLENT - Performance exceptionnelle";
                else if (percentage >= 90)
                    return "TRÈS BIEN - Groupe très performant";
                else if (percentage >= 80)
                    return "BIEN - Performance satisfaisante";
                else if (percentage >= 70)
                    return "CORRECT - À surveiller";
                else if (percentage >= 50)
                    return "MOYEN - Nécessite attention";
                else if (percentage >= 30)
                    return "FAIBLE - Problèmes importants";
                else
                    return "CRITIQUE - Action urgente requise";
            }
            return "DONNÉES INSUFFISANTES";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}