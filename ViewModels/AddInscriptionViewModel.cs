using centre_soutien.Models;
using centre_soutien.DataAccess;
using centre_soutien.Commands;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Linq;
using System.Windows;

namespace centre_soutien.ViewModels
{
    public class AddInscriptionViewModel : INotifyPropertyChanged
    {
        private readonly EtudiantRepository _etudiantRepository;
        private readonly GroupeRepository _groupeRepository;
        private readonly InscriptionRepository _inscriptionRepository;

        #region Propriétés principales

        // Propriété pour indiquer si l'inscription a réussi (pour fermer la fenêtre)
        private bool _inscriptionReussie = false;
        public bool InscriptionReussie
        {
            get => _inscriptionReussie;
            private set { _inscriptionReussie = value; OnPropertyChanged(); }
        }

        // Collections pour les ComboBox
        private ObservableCollection<Etudiant> _allEtudiants;
        public ObservableCollection<Etudiant> AllEtudiants
        {
            get => _allEtudiants;
            set { _allEtudiants = value; OnPropertyChanged(); }
        }

        private ObservableCollection<Groupe> _allGroupes;
        public ObservableCollection<Groupe> AllGroupes
        {
            get => _allGroupes;
            set { _allGroupes = value; OnPropertyChanged(); }
        }

        #endregion

        #region Propriétés pour l'inscription

        private Etudiant? _selectedEtudiant;
        public Etudiant? SelectedEtudiant
        {
            get => _selectedEtudiant;
            set 
            { 
                _selectedEtudiant = value; 
                OnPropertyChanged(); 
                (InscrireCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        private Groupe? _selectedGroupe;
        public Groupe? SelectedGroupe
        {
            get => _selectedGroupe;
            set
            {
                _selectedGroupe = value;
                OnPropertyChanged();
                // Pré-remplir le prix avec le prix standard de la matière du groupe si un groupe est sélectionné
                if (_selectedGroupe?.Matiere != null)
                {
                    PrixConvenuInput = _selectedGroupe.Matiere.PrixStandardMensuel;
                }
                (InscrireCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        private double? _prixConvenuInput;
        public double? PrixConvenuInput
        {
            get => _prixConvenuInput;
            set 
            { 
                _prixConvenuInput = value; 
                OnPropertyChanged(); 
                (InscrireCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        private int? _jourEcheanceInput;
        public int? JourEcheanceInput
        {
            get => _jourEcheanceInput;
            set 
            { 
                _jourEcheanceInput = value; 
                OnPropertyChanged(); 
                (InscrireCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        #endregion

        #region Propriétés pour la création d'un nouvel étudiant

        private bool _isAddingNewStudent = false;
        public bool IsAddingNewStudent
        {
            get => _isAddingNewStudent;
            set 
            { 
                _isAddingNewStudent = value; 
                OnPropertyChanged(); 
                (CreateNewStudentCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (InscrireCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        private string _newStudentNom = string.Empty;
        public string NewStudentNom
        {
            get => _newStudentNom;
            set 
            { 
                _newStudentNom = value; 
                OnPropertyChanged(); 
                (CreateNewStudentCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        private string _newStudentPrenom = string.Empty;
        public string NewStudentPrenom
        {
            get => _newStudentPrenom;
            set 
            { 
                _newStudentPrenom = value; 
                OnPropertyChanged(); 
                (CreateNewStudentCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        private string? _newStudentTelephone;
        public string? NewStudentTelephone
        {
            get => _newStudentTelephone;
            set { _newStudentTelephone = value; OnPropertyChanged(); }
        }

        private string? _newStudentEmail;
        public string? NewStudentEmail
        {
            get => _newStudentEmail;
            set { _newStudentEmail = value; OnPropertyChanged(); }
        }

        private string? _newStudentDateNaissance;
        public string? NewStudentDateNaissance
        {
            get => _newStudentDateNaissance;
            set { _newStudentDateNaissance = value; OnPropertyChanged(); }
        }

        private string? _newStudentAdresse;
        public string? NewStudentAdresse
        {
            get => _newStudentAdresse;
            set { _newStudentAdresse = value; OnPropertyChanged(); }
        }

        private string? _newStudentLycee;
        public string? NewStudentLycee
        {
            get => _newStudentLycee;
            set { _newStudentLycee = value; OnPropertyChanged(); }
        }

        private string? _newStudentNotes;
        public string? NewStudentNotes
        {
            get => _newStudentNotes;
            set { _newStudentNotes = value; OnPropertyChanged(); }
        }

        #endregion

        #region Status et Messages

        private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        #endregion

        #region Commandes

        public ICommand InscrireCommand { get; }
        public ICommand OpenAddStudentCommand { get; }
        public ICommand CreateNewStudentCommand { get; }
        public ICommand CancelNewStudentCommand { get; }
        public ICommand LoadDataCommand { get; }

        #endregion

        #region Constructeur

        public AddInscriptionViewModel(EtudiantRepository etudiantRepo, GroupeRepository groupeRepo, InscriptionRepository inscriptionRepo)
        {
            _etudiantRepository = etudiantRepo;
            _groupeRepository = groupeRepo;
            _inscriptionRepository = inscriptionRepo;

            AllEtudiants = new ObservableCollection<Etudiant>();
            AllGroupes = new ObservableCollection<Groupe>();

            // Initialisation des commandes
            InscrireCommand = new RelayCommand(async param => await InscrireAsync(), CanInscrire);
            OpenAddStudentCommand = new RelayCommand(param => OpenAddStudentForm());
            CreateNewStudentCommand = new RelayCommand(async param => await CreateNewStudentAsync(), CanCreateNewStudent);
            CancelNewStudentCommand = new RelayCommand(param => CancelNewStudentForm());
            LoadDataCommand = new RelayCommand(async param => await LoadInitialDataAsync());

            // Suggérer le jour actuel comme jour d'échéance par défaut
            JourEcheanceInput = DateTime.Now.Day;

            _ = LoadInitialDataAsync(); // Charger les listes pour les ComboBox
        }

        #endregion

        #region Méthodes de chargement des données

        public async Task LoadInitialDataAsync()
        {
            try
            {
                StatusMessage = "Chargement des données...";

                var etudiantsList = await _etudiantRepository.GetAllEtudiantsAsync();
                AllEtudiants = new ObservableCollection<Etudiant>(etudiantsList);

                var groupesList = await _groupeRepository.GetAllGroupesWithDetailsAsync();
                AllGroupes = new ObservableCollection<Groupe>(groupesList);

                StatusMessage = $"Données chargées : {AllEtudiants.Count} étudiants, {AllGroupes.Count} groupes.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ Erreur chargement données : {ex.Message}";
            }
        }

        #endregion

        #region Méthodes pour la gestion des étudiants

        private void OpenAddStudentForm()
        {
            IsAddingNewStudent = true;
            ClearNewStudentForm();
            StatusMessage = "💡 Remplissez les informations du nouvel étudiant ci-dessous.";
        }

        private void CancelNewStudentForm()
        {
            IsAddingNewStudent = false;
            ClearNewStudentForm();
            StatusMessage = "Création d'étudiant annulée.";
        }

        private void ClearNewStudentForm()
        {
            NewStudentNom = string.Empty;
            NewStudentPrenom = string.Empty;
            NewStudentTelephone = null;
            NewStudentEmail = null;
            NewStudentDateNaissance = null;
            NewStudentAdresse = null;
            NewStudentLycee = null;
            NewStudentNotes = null;
        }

        private bool CanCreateNewStudent(object? parameter)
        {
            return IsAddingNewStudent && 
                   !string.IsNullOrWhiteSpace(NewStudentNom) && 
                   !string.IsNullOrWhiteSpace(NewStudentPrenom);
        }

        private async Task CreateNewStudentAsync()
        {
            if (!CanCreateNewStudent(null))
            {
                StatusMessage = "⚠️ Veuillez remplir au minimum le nom et le prénom de l'étudiant.";
                return;
            }

            try
            {
                StatusMessage = "⏳ Création de l'étudiant en cours...";

                var nouvelEtudiant = new Etudiant
                {
                    Nom = NewStudentNom.Trim(),
                    Prenom = NewStudentPrenom.Trim(),
                    Telephone = string.IsNullOrWhiteSpace(NewStudentTelephone) ? null : NewStudentTelephone.Trim(),
                    DateNaissance = string.IsNullOrWhiteSpace(NewStudentDateNaissance) ? null : NewStudentDateNaissance.Trim(),
                    Adresse = string.IsNullOrWhiteSpace(NewStudentAdresse) ? null : NewStudentAdresse.Trim(),
                    Lycee = string.IsNullOrWhiteSpace(NewStudentLycee) ? null : NewStudentLycee.Trim(),
                    Notes = string.IsNullOrWhiteSpace(NewStudentNotes) ? null : NewStudentNotes.Trim(),
                    EstArchive = false

                };

                await _etudiantRepository.AddEtudiantAsync(nouvelEtudiant);

                // Recharger la liste des étudiants
                var etudiantsList = await _etudiantRepository.GetAllEtudiantsAsync();
                AllEtudiants = new ObservableCollection<Etudiant>(etudiantsList);

                // Sélectionner automatiquement le nouvel étudiant créé
                var nouveauEtudiantCree = AllEtudiants
                    .OrderByDescending(e => e.IDEtudiant) // Le plus récent en premier
                    .FirstOrDefault(e => 
                        e.Nom.Equals(nouvelEtudiant.Nom, StringComparison.OrdinalIgnoreCase) && 
                        e.Prenom.Equals(nouvelEtudiant.Prenom, StringComparison.OrdinalIgnoreCase));

                if (nouveauEtudiantCree != null)
                {
                    SelectedEtudiant = nouveauEtudiantCree;
                }

                // Fermer le formulaire de création
                IsAddingNewStudent = false;
                ClearNewStudentForm();

                StatusMessage = $"✅ Étudiant '{nouvelEtudiant.Prenom} {nouvelEtudiant.Nom}' créé avec succès et sélectionné !";

                // Optionnel : Afficher une notification
                MessageBox.Show(
                    $"✅ Nouvel étudiant créé avec succès !\n\n" +
                    $"Nom : {nouvelEtudiant.Prenom} {nouvelEtudiant.Nom}\n" +
                    $"Code : {nouveauEtudiantCree?.Code ?? "En cours de génération"}\n\n" +
                    $"L'étudiant a été automatiquement sélectionné pour l'inscription.",
                    "Étudiant créé",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (InvalidOperationException ex) 
            {
                StatusMessage = $"❌ {ex.Message}";
                MessageBox.Show(ex.Message, "Erreur de validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ Erreur lors de la création de l'étudiant: {ex.Message}";
                MessageBox.Show(
                    $"Une erreur inattendue s'est produite :\n{ex.Message}",
                    "Erreur",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        #endregion

        #region Méthodes pour l'inscription

        private bool CanInscrire(object? parameter)
        {
            return SelectedEtudiant != null &&
                   SelectedGroupe != null &&
                   PrixConvenuInput.HasValue && PrixConvenuInput.Value >= 0 &&
                   JourEcheanceInput.HasValue && JourEcheanceInput.Value >= 1 && JourEcheanceInput.Value <= 31 &&
                   !IsAddingNewStudent; // Ne pas permettre l'inscription si on est en train de créer un étudiant
        }

        private async Task InscrireAsync()
        {
            if (!CanInscrire(null))
            {
                StatusMessage = "⚠️ Veuillez remplir tous les champs correctement et terminer la création d'étudiant si nécessaire.";
                return;
            }

            try
            {
                StatusMessage = "⏳ Inscription en cours...";

                var nouvelleInscription = new Inscription
                {
                    IDEtudiant = SelectedEtudiant!.IDEtudiant,
                    IDGroupe = SelectedGroupe!.IDGroupe,
                    PrixConvenuMensuel = PrixConvenuInput!.Value,
                    JourEcheanceMensuelle = JourEcheanceInput!.Value,
                    // DateInscription et EstActif seront gérés par le Repository
                };

                await _inscriptionRepository.AddInscriptionAsync(nouvelleInscription);
                
                StatusMessage = $"✅ Inscription réussie ! {SelectedEtudiant.NomComplet} inscrit(e) au groupe {SelectedGroupe.NomDescriptifGroupe}.";
                InscriptionReussie = true; // Pour signaler à la vue de se fermer
            }
            catch (InvalidOperationException ex) // Ex: Étudiant déjà inscrit
            {
                StatusMessage = $"❌ {ex.Message}";
                MessageBox.Show(ex.Message, "Inscription impossible", MessageBoxButton.OK, MessageBoxImage.Warning);
                InscriptionReussie = false;
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ Erreur lors de l'inscription: {ex.Message}";
                MessageBox.Show(
                    $"Une erreur inattendue s'est produite :\n{ex.Message}",
                    "Erreur d'inscription",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                InscriptionReussie = false;
            }
        }

        #endregion

        #region Méthodes utilitaires

        /// <summary>
        /// Réinitialise le formulaire pour une nouvelle inscription
        /// </summary>
        public void ResetForm()
        {
            SelectedEtudiant = null;
            SelectedGroupe = null;
            PrixConvenuInput = null;
            JourEcheanceInput = DateTime.Now.Day;
            IsAddingNewStudent = false;
            ClearNewStudentForm();
            StatusMessage = string.Empty;
            InscriptionReussie = false;
        }

        /// <summary>
        /// Pré-sélectionne un étudiant (utile si appelé depuis une autre vue)
        /// </summary>
        public void PreselectEtudiant(int etudiantId)
        {
            var etudiant = AllEtudiants?.FirstOrDefault(e => e.IDEtudiant == etudiantId);
            if (etudiant != null)
            {
                SelectedEtudiant = etudiant;
                StatusMessage = $"Étudiant {etudiant.NomComplet} pré-sélectionné.";
            }
        }

        /// <summary>
        /// Pré-sélectionne un groupe (utile si appelé depuis une autre vue)
        /// </summary>
        public void PreselectGroupe(int groupeId)
        {
            var groupe = AllGroupes?.FirstOrDefault(g => g.IDGroupe == groupeId);
            if (groupe != null)
            {
                SelectedGroupe = groupe;
                StatusMessage = $"Groupe {groupe.NomDescriptifGroupe} pré-sélectionné.";
            }
        }

        /// <summary>
        /// Valide le format d'une date (optionnel)
        /// </summary>
        private bool IsValidDateFormat(string? dateString)
        {
            if (string.IsNullOrWhiteSpace(dateString)) return true; // Date optionnelle
            return DateTime.TryParse(dateString, out _);
        }

        /// <summary>
        /// Valide le format d'un email (optionnel)
        /// </summary>
        private bool IsValidEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email)) return true; // Email optionnel
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}