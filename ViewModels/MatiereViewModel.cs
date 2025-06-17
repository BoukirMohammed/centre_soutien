using centre_soutien.Models;
using centre_soutien.DataAccess;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using centre_soutien.Commands;
using centre_soutien.Services;

namespace centre_soutien.ViewModels
{


    public class MatiereViewModel : INotifyPropertyChanged
    {
        private readonly MatiereRepository _matiereRepository;

        private ObservableCollection<Matiere> _matieres;
        public ObservableCollection<Matiere> Matieres
        {
            get => _matieres;
            set { _matieres = value; OnPropertyChanged(); }
        }
        public bool CanUserArchive => CurrentUserSession.IsAdmin; // Ou CurrentUserSession.IsAdmin || CurrentUserSession.IsSecretaire si la secrétaire peut aussi archiver certains types.

        private Matiere _selectedMatiere;
        public Matiere SelectedMatiere
        {
            get => _selectedMatiere;
            set
            {
                _selectedMatiere = value;
                OnPropertyChanged();
                if (_selectedMatiere != null)
                {
                    NomMatiereInput = _selectedMatiere.NomMatiere;
                    PrixInput = _selectedMatiere.PrixStandardMensuel;
                    DescriptionInput = _selectedMatiere.Description;
                }
                else
                {
                    ClearInputFields();
                }
                (AddMatiereCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (UpdateMatiereCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (ArchiveMatiereCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        // Champs pour le formulaire
        private string _nomMatiereInput;
        public string NomMatiereInput
        {
            get => _nomMatiereInput;
            set { _nomMatiereInput = value; OnPropertyChanged(); (AddMatiereCommand as RelayCommand)?.RaiseCanExecuteChanged(); (UpdateMatiereCommand as RelayCommand)?.RaiseCanExecuteChanged(); }
        }

        private double? _prixInput; // Nullable pour permettre un champ vide initialement
        public double? PrixInput
        {
            get => _prixInput;
            set { _prixInput = value; OnPropertyChanged(); (AddMatiereCommand as RelayCommand)?.RaiseCanExecuteChanged(); (UpdateMatiereCommand as RelayCommand)?.RaiseCanExecuteChanged(); }
        }

        private string _descriptionInput;
        public string DescriptionInput
        {
            get => _descriptionInput;
            set { _descriptionInput = value; OnPropertyChanged(); }
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        // Commandes
        public ICommand LoadMatieresCommand { get; }
        public ICommand AddMatiereCommand { get; }
        public ICommand UpdateMatiereCommand { get; }
        public ICommand ArchiveMatiereCommand { get; }
        public ICommand ClearFormCommand { get; }

        public MatiereViewModel()
        {
            _matiereRepository = new MatiereRepository();
            Matieres = new ObservableCollection<Matiere>();

            LoadMatieresCommand = new RelayCommand(async param => await LoadMatieresAsync());
            AddMatiereCommand = new RelayCommand(async param => await AddMatiereAsync(), CanAddOrUpdateMatiere);
            UpdateMatiereCommand = new RelayCommand(async param => await UpdateMatiereAsync(), CanUpdateOrArchiveMatiere);
            ArchiveMatiereCommand = new RelayCommand(
                async param => await ArchiveMatiereAsync(),
                param => SelectedMatiere != null && CanUserArchive // Le bouton ne s'active que si un étudiant est sélectionné ET l'utilisateur a le droit
            );
            ClearFormCommand = new RelayCommand(param => ClearInputFieldsAndSelection());

            _ = LoadMatieresAsync();
        }

        public async Task LoadMatieresAsync()
        {
            try
            {
                var matieresList = await _matiereRepository.GetAllMatieresAsync();
                Matieres = new ObservableCollection<Matiere>(matieresList);
                StatusMessage = $"Chargement de {Matieres.Count} matières réussi.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur lors du chargement des matières: {ex.Message}";
            }
        }

        private bool CanAddOrUpdateMatiere(object parameter)
        {
            return !string.IsNullOrWhiteSpace(NomMatiereInput) && PrixInput.HasValue && PrixInput.Value >= 0;
        }

        private async Task AddMatiereAsync()
        {
            if (!CanAddOrUpdateMatiere(null))
            {
                StatusMessage = "Veuillez entrer un nom et un prix valide (positif).";
                return;
            }

            var nouvelleMatiere = new Matiere
            {
                NomMatiere = NomMatiereInput,
                PrixStandardMensuel = PrixInput.Value, // Value car PrixInput est double?
                Description = DescriptionInput,
                EstArchivee = false
            };

            try
            {
                await _matiereRepository.AddMatiereAsync(nouvelleMatiere);
                await LoadMatieresAsync();
                ClearInputFieldsAndSelection();
                StatusMessage = $"Matière '{nouvelleMatiere.NomMatiere}' ajoutée.";
            }
            catch (InvalidOperationException ex) { StatusMessage = ex.Message; }
            catch (Exception ex) { StatusMessage = $"Erreur ajout matière: {ex.Message}"; }
        }
        
        private bool CanUpdateOrArchiveMatiere(object parameter)
        {
            return SelectedMatiere != null && !string.IsNullOrWhiteSpace(NomMatiereInput) && PrixInput.HasValue && PrixInput.Value >= 0;
        }
        private bool CanArchiveMatiere(object parameter)
        {
            return SelectedMatiere != null;
        }


        private async Task UpdateMatiereAsync()
        {
            if (!CanUpdateOrArchiveMatiere(null) || SelectedMatiere == null) return;

            var matiereAMettreAJour = new Matiere
            {
                IDMatiere = SelectedMatiere.IDMatiere,
                NomMatiere = NomMatiereInput,
                PrixStandardMensuel = PrixInput.Value,
                Description = DescriptionInput,
                EstArchivee = SelectedMatiere.EstArchivee
            };

            try
            {
                await _matiereRepository.UpdateMatiereAsync(matiereAMettreAJour);
                await LoadMatieresAsync();
                ClearInputFieldsAndSelection();
                StatusMessage = $"Matière '{matiereAMettreAJour.NomMatiere}' mise à jour.";
            }
            catch (InvalidOperationException ex) { StatusMessage = ex.Message; }
            catch (Exception ex) { StatusMessage = $"Erreur MàJ matière: {ex.Message}"; }
        }

        private async Task ArchiveMatiereAsync()
        {
            if (!CanArchiveMatiere(null) || SelectedMatiere == null) return;

            try
            {
                await _matiereRepository.ArchiveMatiereAsync(SelectedMatiere.IDMatiere);
                await LoadMatieresAsync();
                ClearInputFieldsAndSelection();
                StatusMessage = $"Matière '{SelectedMatiere.NomMatiere}' archivée.";
            }
            catch (InvalidOperationException ex) { StatusMessage = ex.Message; }
            catch (Exception ex) { StatusMessage = $"Erreur archivage: {ex.Message}"; }
        }

        private void ClearInputFields()
        {
            NomMatiereInput = string.Empty;
            PrixInput = null;
            DescriptionInput = string.Empty;
        }

        private void ClearInputFieldsAndSelection()
        {
            ClearInputFields();
            SelectedMatiere = null;
            StatusMessage = string.Empty;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}