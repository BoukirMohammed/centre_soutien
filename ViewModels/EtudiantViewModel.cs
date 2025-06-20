using centre_soutien.Models;
using centre_soutien.DataAccess;
using centre_soutien.Commands; // Assure-toi que ce using pointe vers ta classe RelayCommand
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using centre_soutien.Services;
using centre_soutien.Services.PDF; // Pour CurrentUserSession


namespace centre_soutien.ViewModels
{
    public class EtudiantViewModel : INotifyPropertyChanged
    {
        private readonly EtudiantRepository _etudiantRepository;

        private ObservableCollection<Etudiant> _etudiants;

        public ObservableCollection<Etudiant> Etudiants
        {
            get => _etudiants;
            set
            {
                _etudiants = value;
                OnPropertyChanged();
            }
        }

        private Etudiant? _selectedEtudiant; // Nullable
        public bool CanUserArchive => CurrentUserSession.IsAdmin;

        public Etudiant? SelectedEtudiant
        {
            get => _selectedEtudiant;
            set
            {
                _selectedEtudiant = value;
                OnPropertyChanged();
                if (_selectedEtudiant != null)
                {
                    NomInput = _selectedEtudiant.Nom;
                    PrenomInput = _selectedEtudiant.Prenom;
                    DateNaissanceInput = _selectedEtudiant.DateNaissance; // Peut nécessiter une conversion si DateTime
                    AdresseInput = _selectedEtudiant.Adresse;
                    TelephoneInput = _selectedEtudiant.Telephone;
                    LyceeInput = _selectedEtudiant.Lycee;
                    NotesInput = _selectedEtudiant.Notes;
                }
                else
                {
                    ClearInputFields();
                }

                (AddEtudiantCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (UpdateEtudiantCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (ArchiveEtudiantCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (DeleteEtudiantCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        private readonly IPdfExportService _pdfService;

        public ICommand ExportPdfCommand { get; }

        // Champs pour le formulaire
        private string _nomInput = string.Empty;

        public string NomInput
        {
            get => _nomInput;
            set
            {
                _nomInput = value;
                OnPropertyChanged();
                (AddEtudiantCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (UpdateEtudiantCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        private string _prenomInput = string.Empty;

        public string PrenomInput
        {
            get => _prenomInput;
            set
            {
                _prenomInput = value;
                OnPropertyChanged();
                (AddEtudiantCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (UpdateEtudiantCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        private string? _dateNaissanceInput; // Format "YYYY-MM-DD"

        public string? DateNaissanceInput
        {
            get => _dateNaissanceInput;
            set
            {
                _dateNaissanceInput = value;
                OnPropertyChanged();
            }
        }

        private string? _adresseInput;

        public string? AdresseInput
        {
            get => _adresseInput;
            set
            {
                _adresseInput = value;
                OnPropertyChanged();
            }
        }

        private string? _telephoneInput;

        public string? TelephoneInput
        {
            get => _telephoneInput;
            set
            {
                _telephoneInput = value;
                OnPropertyChanged();
            }
        }

        private string? _lyceeInput;

        public string? LyceeInput
        {
            get => _lyceeInput;
            set
            {
                _lyceeInput = value;
                OnPropertyChanged();
            }
        }

        private string? _notesInput;

        public string? NotesInput
        {
            get => _notesInput;
            set
            {
                _notesInput = value;
                OnPropertyChanged();
            }
        }

        private string _statusMessage = string.Empty;

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        // Commandes
        public ICommand LoadEtudiantsCommand { get; }
        public ICommand AddEtudiantCommand { get; }
        public ICommand UpdateEtudiantCommand { get; }
        public ICommand ArchiveEtudiantCommand { get; }
        public ICommand ClearFormCommand { get; }
        public ICommand DeleteEtudiantCommand { get; }
        public ICommand ForceDeleteEtudiantCommand { get; }


        public EtudiantViewModel()
        {
            _etudiantRepository = new EtudiantRepository();
            _pdfService = new PdfExportService();
            Etudiants = new ObservableCollection<Etudiant>();
            DeleteEtudiantCommand = new RelayCommand(
                async param => await DeleteEtudiantAsync(),
                param => SelectedEtudiant != null && CanUserArchive // Même permission que l'archivage
            );
            ForceDeleteEtudiantCommand = new RelayCommand(
                async param => await ForceDeleteEtudiantAsync(),
                param => SelectedEtudiant != null && CanUserArchive
            );
            LoadEtudiantsCommand = new RelayCommand(async param => await LoadEtudiantsAsync());
            AddEtudiantCommand = new RelayCommand(async param => await AddEtudiantAsync(), CanAddOrUpdateEtudiant);
            UpdateEtudiantCommand =
                new RelayCommand(async param => await UpdateEtudiantAsync(), CanUpdateOrArchiveEtudiant);
            ArchiveEtudiantCommand = new RelayCommand(
                async param => await ArchiveEtudiantAsync(),
                param => SelectedEtudiant != null && CanUserArchive
            );
            ClearFormCommand = new RelayCommand(param => ClearInputFieldsAndSelection());

            ExportPdfCommand = new RelayCommand(async param => await ExportToPdfAsync());
        }

        private async Task ExportToPdfAsync()
        {
            try
            {
                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Fichiers PDF (*.pdf)|*.pdf",
                    DefaultExt = "pdf",
                    FileName = $"Liste_Etudiants_{DateTime.Now:yyyyMMdd_HHmm}.pdf"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    StatusMessage = "Export PDF en cours...";

                    bool success = await _pdfService.ExportEtudiantsListAsync(
                        Etudiants.ToList(), saveDialog.FileName);

                    if (success)
                    {
                        StatusMessage = "Export PDF réussi !";
                        MessageBox.Show("Export terminé avec succès !", "Export réussi",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        StatusMessage = "Erreur lors de l'export PDF";
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur : {ex.Message}";
            }
        }

        public async Task LoadEtudiantsAsync()
        {
            try
            {
                var etudiantsList = await _etudiantRepository.GetAllEtudiantsAsync();
                Etudiants = new ObservableCollection<Etudiant>(etudiantsList);
                StatusMessage = $"Chargement de {Etudiants.Count} étudiants réussi.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur chargement étudiants: {ex.Message}";
            }
        }

        private bool CanAddOrUpdateEtudiant(object? parameter)
        {
            return !string.IsNullOrWhiteSpace(NomInput) && !string.IsNullOrWhiteSpace(PrenomInput);
            // Tu pourrais ajouter d'autres validations ici (ex: format date de naissance)
        }

        private async Task AddEtudiantAsync()
        {
            if (!CanAddOrUpdateEtudiant(null))
            {
                StatusMessage = "Nom et prénom sont requis.";
                return;
            }

            var nouvelEtudiant = new Etudiant
            {
                Nom = NomInput,
                Prenom = PrenomInput,
                DateNaissance = DateNaissanceInput, // Valider le format si nécessaire
                Adresse = AdresseInput,
                Telephone = TelephoneInput,
                Lycee = LyceeInput,
                Notes = NotesInput,
                EstArchive = false
                // DateInscriptionSysteme est gérée par le Repository ou le modèle
            };

            try
            {
                await _etudiantRepository.AddEtudiantAsync(nouvelEtudiant);
                await LoadEtudiantsAsync();
                ClearInputFieldsAndSelection();
                StatusMessage = $"Étudiant '{nouvelEtudiant.Prenom} {nouvelEtudiant.Nom}' ajouté.";
            }
            catch (InvalidOperationException ex) // Pour les erreurs d'unicité (ex: téléphone)
            {
                StatusMessage = ex.Message;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur ajout étudiant: {ex.Message}";
            }
        }

        private bool CanUpdateOrArchiveEtudiant(object? parameter)
        {
            return SelectedEtudiant != null && !string.IsNullOrWhiteSpace(NomInput) &&
                   !string.IsNullOrWhiteSpace(PrenomInput);
        }

        private bool CanArchiveEtudiant(object? parameter)
        {
            return SelectedEtudiant != null;
        }

        private async Task UpdateEtudiantAsync()
        {
            if (!CanUpdateOrArchiveEtudiant(null) || SelectedEtudiant == null) return;

            var etudiantAMettreAJour = new Etudiant
            {
                IDEtudiant = SelectedEtudiant.IDEtudiant,
                Nom = NomInput,
                Prenom = PrenomInput,
                DateNaissance = DateNaissanceInput,
                Adresse = AdresseInput,
                Telephone = TelephoneInput,
                Lycee = LyceeInput,
                Notes = NotesInput,
                DateInscriptionSysteme =
                    SelectedEtudiant.DateInscriptionSysteme, // Conserver la date d'inscription originale
                EstArchive = SelectedEtudiant.EstArchive
            };

            try
            {
                await _etudiantRepository.UpdateEtudiantAsync(etudiantAMettreAJour);
                await LoadEtudiantsAsync();
                ClearInputFieldsAndSelection();
                StatusMessage = $"Étudiant '{etudiantAMettreAJour.Prenom} {etudiantAMettreAJour.Nom}' mis à jour.";
            }
            catch (InvalidOperationException ex)
            {
                StatusMessage = ex.Message;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur MàJ étudiant: {ex.Message}";
            }
        }

        private async Task ArchiveEtudiantAsync()
        {
            if (!CanArchiveEtudiant(null) || SelectedEtudiant == null) return;

            try
            {
                await _etudiantRepository.ArchiveEtudiantAsync(SelectedEtudiant.IDEtudiant);
                await LoadEtudiantsAsync();
                ClearInputFieldsAndSelection();
                StatusMessage = $"Étudiant '{SelectedEtudiant.Prenom} {SelectedEtudiant.Nom}' archivé.";
            }
            catch (InvalidOperationException ex) // Pour les erreurs du repo (ex: inscriptions actives)
            {
                StatusMessage = ex.Message;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur archivage étudiant: {ex.Message}";
            }
        }

        private void ClearInputFields()
        {
            NomInput = string.Empty;
            PrenomInput = string.Empty;
            DateNaissanceInput = null;
            AdresseInput = null;
            TelephoneInput = null;
            LyceeInput = null;
            NotesInput = null;
        }

        private void ClearInputFieldsAndSelection()
        {
            ClearInputFields();
            SelectedEtudiant = null;
            StatusMessage = string.Empty;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

// Remplacez les méthodes DeleteEtudiantAsync et ajoutez ForceDeleteEtudiantAsync dans votre EtudiantViewModel

        /// <summary>
        /// Suppression simple - avec gestion intelligente des erreurs
        /// </summary>
        private async Task DeleteEtudiantAsync()
        {
            if (SelectedEtudiant == null) return;

            try
            {
                // Sauvegarder les infos pour le message de statut
                string nomComplet = $"{SelectedEtudiant.Prenom} {SelectedEtudiant.Nom}";

                // Obtenir un résumé des données associées
                string dataSummary = await _etudiantRepository.GetEtudiantDataSummaryAsync(SelectedEtudiant.IDEtudiant);

                // Si des données sont détectées, prévenir l'utilisateur
                if (!dataSummary.StartsWith("✅"))
                {
                    var preResult = MessageBox.Show(
                        $"📊 Analyse des données pour {nomComplet} :\n\n" +
                        $"{dataSummary}\n\n" +
                        "Voulez-vous quand même tenter la suppression ?",
                        "Données associées détectées",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (preResult != MessageBoxResult.Yes)
                        return;
                }

                // Tentative de suppression
                await _etudiantRepository.DeleteEtudiantAsync(SelectedEtudiant.IDEtudiant);

                // Recharger la liste des étudiants
                await LoadEtudiantsAsync();

                // Vider le formulaire
                ClearInputFieldsAndSelection();

                StatusMessage = $"✅ Étudiant '{nomComplet}' supprimé définitivement.";
            }
            catch (InvalidOperationException ex)
            {
                // Erreurs métier avec proposition d'alternatives
                StatusMessage = "❌ " + ex.Message.Split('\n')[0]; // Première ligne seulement pour le status

                var result = MessageBox.Show(
                    $"{ex.Message}\n\n" +
                    "Que souhaitez-vous faire ?\n\n" +
                    "• OUI : Archiver l'étudiant (recommandé)\n" +
                    "• NON : Annuler l'opération\n" +
                    "• ANNULER : Voir les options de suppression forcée",
                    "Suppression impossible",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                switch (result)
                {
                    case MessageBoxResult.Yes:
                        // Basculer vers l'archivage
                        await ArchiveEtudiantAsync();
                        break;

                    case MessageBoxResult.Cancel:
                        // Proposer la suppression forcée si admin
                        if (CanUserArchive)
                        {
                            var forceResult = MessageBox.Show(
                                "Voulez-vous effectuer une suppression forcée ?\n\n" +
                                "⚠️ Ceci supprimera l'étudiant ET toutes ses données associées !",
                                "Suppression forcée",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Stop);

                            if (forceResult == MessageBoxResult.Yes)
                            {
                                await ForceDeleteEtudiantAsync();
                            }
                        }
                        else
                        {
                            MessageBox.Show("Vous n'avez pas les droits pour la suppression forcée.",
                                "Accès refusé", MessageBoxButton.OK, MessageBoxImage.Information);
                        }

                        break;
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ Erreur suppression : {ex.Message}";

                MessageBox.Show(
                    $"Une erreur inattendue s'est produite :\n{ex.Message}",
                    "Erreur",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Suppression forcée avec toutes les données associées
        /// </summary>
        private async Task ForceDeleteEtudiantAsync()
        {
            if (SelectedEtudiant == null) return;

            try
            {
                string nomComplet = $"{SelectedEtudiant.Prenom} {SelectedEtudiant.Nom}";

                // Obtenir le résumé des données qui seront supprimées
                string dataSummary = await _etudiantRepository.GetEtudiantDataSummaryAsync(SelectedEtudiant.IDEtudiant);

                var result = MessageBox.Show(
                    $"🚨 SUPPRESSION FORCÉE 🚨\n\n" +
                    $"Étudiant : {nomComplet}\n\n" +
                    $"Cette action va supprimer DÉFINITIVEMENT :\n" +
                    $"• L'étudiant lui-même\n" +
                    $"• Toutes ses inscriptions\n" +
                    $"• Tous ses paiements\n" +
                    $"• Son historique complet\n\n" +
                    $"Analyse actuelle :\n{dataSummary.Replace("💡 Recommandation : Utilisez l'archivage plutôt que la suppression.", "")}\n" +
                    $"⚠️ CETTE ACTION EST IRRÉVERSIBLE !\n\n" +
                    $"Êtes-vous absolument certain ?",
                    "SUPPRESSION FORCÉE",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Stop);

                if (result == MessageBoxResult.Yes)
                {
                    // Utiliser la méthode ForceDeleteEtudiantAsync du repository
                    await _etudiantRepository.ForceDeleteEtudiantAsync(SelectedEtudiant.IDEtudiant);

                    await LoadEtudiantsAsync();
                    ClearInputFieldsAndSelection();

                    StatusMessage = $"💀 Étudiant '{nomComplet}' et toutes ses données supprimés définitivement.";

                    MessageBox.Show(
                        $"✅ Suppression forcée terminée !\n\n" +
                        $"L'étudiant {nomComplet} et toutes ses données associées ont été supprimés définitivement.",
                        "Suppression réussie",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ Erreur suppression forcée : {ex.Message}";

                MessageBox.Show(
                    $"Erreur lors de la suppression forcée :\n{ex.Message}",
                    "Erreur",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}