using System.Windows; // Nécessaire pour la classe Window

namespace centre_soutien // Assure-toi que le namespace est correct
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // DataContext = new ViewModels.MainViewModel();
            
        }
    }
}