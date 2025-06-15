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
}