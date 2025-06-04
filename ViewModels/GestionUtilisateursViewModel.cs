using centre_soutien.Models;
using centre_soutien.DataAccess;
using centre_soutien.Commands; // Pour RelayCommand
using System;
using System.Collections.Generic; // Pour List<string> pour les rôles
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security; // Pour SecureString si tu l'utilises pour la saisie de mdp
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Controls; // Pour PasswordBox si passé en paramètre

namespace centre_soutien.ViewModels
{
    public class GestionUtilisateursViewModel : INotifyPropertyChanged
    {
        private readonly UtilisateurRepository _utilisateurRepository;

        private ObservableCollection<Utilisateur> _utilisateurs;
        public ObservableCollection<Utilisateur> Utilisateurs
        {
            get => _utilisateurs;
            set { _utilisateurs = value; OnPropertyChanged(); }
        }

        private Utilisateur? _selectedUtilisateur;
        public Utilisateur? SelectedUtilisateur
        {
            get => _selectedUtilisateur;
            set
            {
                _selectedUtilisateur = value;
                OnPropertyChanged();
                IsEditing = false; // Sortir du mode édition si on change de sélection
                if (_selectedUtilisateur != null)
                {
                    LoginInput = _selectedUtilisateur.Login;
                    NomCompletInput = _selectedUtilisateur.NomComplet;
                    SelectedRoleInput = _selectedUtilisateur.Role;
                    IsActifInput = _selectedUtilisateur.EstActif;
                    // Ne pas charger le mot de passe dans le formulaire
                    ClearPasswordFields(); 
                }
                else
                {
                    ClearInputFields();
                }
                // Mettre à jour CanExecute des commandes
                // (UpdateUtilisateurCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (ChangePasswordCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (ToggleActivationCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        // --- Propriétés pour le formulaire ---
        private string _loginInput = string.Empty;
        public string LoginInput { get => _loginInput; set { _loginInput = value; OnPropertyChanged(); ValidateForm(); } }

        private string? _nomCompletInput;
        public string? NomCompletInput { get => _nomCompletInput; set { _nomCompletInput = value; OnPropertyChanged(); ValidateForm(); } }

        public List<string> RolesDisponibles => new List<string> { "Admin", "Secretaire" }; // Pour la ComboBox des rôles

        private string? _selectedRoleInput;
        public string? SelectedRoleInput { get => _selectedRoleInput; set { _selectedRoleInput = value; OnPropertyChanged(); ValidateForm(); } }

        private bool _isActifInput;
        public bool IsActifInput { get => _isActifInput; set { _isActifInput = value; OnPropertyChanged(); } }
        
        // Pour le mot de passe (utilisé à l'ajout ou au changement)
        // On ne stocke pas directement le mot de passe, on le récupère du PasswordBox
        public string ToggleActivationButtonText => SelectedUtilisateur != null && SelectedUtilisateur.EstActif ? "Désactiver" : "Activer";


        private bool _isEditing = false; // Pour savoir si on est en mode ajout ou édition
        public bool IsEditing { get => _isEditing; set { _isEditing = value; OnPropertyChanged(); ValidateForm(); } }


        private string _statusMessage = string.Empty;
        public string StatusMessage { get => _statusMessage; set { _statusMessage = value; OnPropertyChanged(); } }

        // Commandes
        public ICommand LoadUtilisateursCommand { get; }
        public ICommand PrepareAddUtilisateurCommand { get; } // Pour mettre le formulaire en mode ajout
        public ICommand SaveUtilisateurCommand { get; } // S'occupe de l'ajout ou de la modif
        public ICommand ChangePasswordCommand { get; } // Ouvre un dialogue ou active des champs pour changer le mdp
        public ICommand ToggleActivationCommand { get; }
        public ICommand CancelEditCommand { get; } // Pour annuler l'ajout/modif


        public GestionUtilisateursViewModel()
        {
            _utilisateurRepository = new UtilisateurRepository();
            Utilisateurs = new ObservableCollection<Utilisateur>();

            LoadUtilisateursCommand = new RelayCommand(async param => await LoadUtilisateursAsync());
            PrepareAddUtilisateurCommand = new RelayCommand(param => PrepareForAdd());
            SaveUtilisateurCommand = new RelayCommand(async parameter => await SaveUtilisateurAsync(parameter), CanSaveUtilisateur);
            ChangePasswordCommand = new RelayCommand(async parameter => await ExecuteChangePasswordAsync(parameter), CanChangePasswordOrToggleActivation);
            ToggleActivationCommand = new RelayCommand(async param => await ToggleActivationAsync(), CanChangePasswordOrToggleActivation);
            CancelEditCommand = new RelayCommand(param => CancelCurrentEdit());


            _ = LoadUtilisateursAsync();
        }

        public async Task LoadUtilisateursAsync()
        {
            try
            {
                var userList = await _utilisateurRepository.GetAllUtilisateursAsync();
                Utilisateurs = new ObservableCollection<Utilisateur>(userList);
                StatusMessage = $"Chargement de {Utilisateurs.Count} utilisateurs réussi.";
            }
            catch (Exception ex) { StatusMessage = $"Erreur chargement utilisateurs: {ex.Message}"; }
        }

        private void PrepareForAdd()
        {
            SelectedUtilisateur = null; // Désélectionner pour indiquer un nouvel ajout
            IsEditing = true;
            ClearInputFields();
            IsActifInput = true; // Par défaut actif
            StatusMessage = "Saisie d'un nouvel utilisateur...";
        }

        private void ClearInputFields()
        {
            LoginInput = string.Empty;
            NomCompletInput = string.Empty;
            SelectedRoleInput = RolesDisponibles.FirstOrDefault(); // Rôle par défaut
            IsActifInput = true;
            ClearPasswordFields();
        }
        
        private void ClearPasswordFields()
        {
            // La vue devra s'occuper de vider les PasswordBox
            OnPropertyChanged(nameof(NewPasswordForAddOrChange)); // Pour notifier la vue si elle est liée à ça
        }

        // Propriété pour que la vue lie le PasswordBox (via CommandParameter)
        public object? NewPasswordForAddOrChange { private get; set; } 

        private bool CanSaveUtilisateur(object? parameter)
        {
            return IsEditing &&
                   !string.IsNullOrWhiteSpace(LoginInput) &&
                   !string.IsNullOrWhiteSpace(NomCompletInput) &&
                   !string.IsNullOrWhiteSpace(SelectedRoleInput) &&
                   (SelectedUtilisateur == null); // Pour l'ajout, on a besoin du mot de passe (qui viendra du paramètre)
                                                  // Pour la modification, le mot de passe n'est pas obligatoire ici
        }
        
        private void ValidateForm() // Appelé par les setters des inputs
        {
             (SaveUtilisateurCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }


        private async Task SaveUtilisateurAsync(object? passwordBoxParameter)
        {
            if (!CanSaveUtilisateur(passwordBoxParameter))
            {
                 StatusMessage = "Veuillez remplir tous les champs obligatoires (Login, Nom, Rôle). Le mot de passe est requis pour un nouvel utilisateur.";
                return;
            }
            
            string? password = null;
            if (passwordBoxParameter is PasswordBox pdb)
            {
                SecureString? securePassword = pdb.SecurePassword;
                if (securePassword != null && securePassword.Length > 0)
                {
                    IntPtr bstr = System.Runtime.InteropServices.Marshal.SecureStringToBSTR(securePassword);
                    try { password = System.Runtime.InteropServices.Marshal.PtrToStringBSTR(bstr); }
                    finally { System.Runtime.InteropServices.Marshal.ZeroFreeBSTR(bstr); }
                }
                pdb.Clear(); // Toujours vider après usage
            }


            if (SelectedUtilisateur == null) // Ajout d'un nouvel utilisateur
            {
                if (string.IsNullOrWhiteSpace(password))
                {
                    StatusMessage = "Le mot de passe est requis pour un nouvel utilisateur.";
                    return;
                }
                var newUser = new Utilisateur
                {
                    Login = LoginInput,
                    NomComplet = NomCompletInput,
                    Role = SelectedRoleInput,
                    EstActif = IsActifInput 
                };
                try
                {
                    await _utilisateurRepository.AddUtilisateurAsync(newUser, password);
                    StatusMessage = $"Utilisateur '{newUser.Login}' ajouté.";
                    IsEditing = false;
                    await LoadUtilisateursAsync();
                    ClearInputFieldsAndSelection();
                }
                catch (InvalidOperationException ex) { StatusMessage = ex.Message; }
                catch (Exception ex) { StatusMessage = $"Erreur ajout utilisateur: {ex.Message}"; }
            }
            else // Modification d'un utilisateur existant
            {
                var userToUpdate = new Utilisateur
                {
                    IDUtilisateur = SelectedUtilisateur.IDUtilisateur,
                    Login = LoginInput,
                    NomComplet = NomCompletInput,
                    Role = SelectedRoleInput,
                    EstActif = IsActifInput,
                    MotDePasseHashe = SelectedUtilisateur.MotDePasseHashe // On ne change pas le mdp ici
                };
                try
                {
                    await _utilisateurRepository.UpdateUtilisateurAsync(userToUpdate);
                    StatusMessage = $"Utilisateur '{userToUpdate.Login}' mis à jour.";
                    IsEditing = false;
                    await LoadUtilisateursAsync();
                    ClearInputFieldsAndSelection();
                }
                catch (InvalidOperationException ex) { StatusMessage = ex.Message; }
                catch (Exception ex) { StatusMessage = $"Erreur MàJ utilisateur: {ex.Message}"; }
            }
        }
        
        private bool CanChangePasswordOrToggleActivation(object? parameter)
        {
            return SelectedUtilisateur != null;
        }

        private async Task ExecuteChangePasswordAsync(object? passwordBoxParameter)
        {
            if (!CanChangePasswordOrToggleActivation(null) || SelectedUtilisateur == null) return;

            string? newPassword = null;
            if (passwordBoxParameter is PasswordBox pdb)
            {
                SecureString? securePassword = pdb.SecurePassword;
                if (securePassword != null && securePassword.Length > 0)
                {
                    IntPtr bstr = System.Runtime.InteropServices.Marshal.SecureStringToBSTR(securePassword);
                    try { newPassword = System.Runtime.InteropServices.Marshal.PtrToStringBSTR(bstr); }
                    finally { System.Runtime.InteropServices.Marshal.ZeroFreeBSTR(bstr); }
                }
                pdb.Clear();
            }

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                StatusMessage = "Le nouveau mot de passe ne peut pas être vide.";
                return;
            }

            try
            {
                await _utilisateurRepository.ChangePasswordAsync(SelectedUtilisateur.IDUtilisateur, newPassword);
                StatusMessage = $"Mot de passe de '{SelectedUtilisateur.Login}' changé.";
                // Ne pas recharger la liste ici, juste un changement de mdp
                ClearPasswordFields(); // Pour la vue
            }
            catch (Exception ex) { StatusMessage = $"Erreur changement mot de passe: {ex.Message}"; }
        }


        private async Task ToggleActivationAsync()
        {
            if (!CanChangePasswordOrToggleActivation(null) || SelectedUtilisateur == null) return;

            try
            {
                var userToToggle = await _utilisateurRepository.GetUtilisateurByIdAsync(SelectedUtilisateur.IDUtilisateur);
                if(userToToggle == null) { StatusMessage = "Utilisateur non trouvé."; return;}

                // S'assurer qu'un admin ne se désactive pas lui-même s'il est le seul admin actif
                if (userToToggle.Role == "Admin" && userToToggle.EstActif)
                {
                    var autresAdminsActifs = await _utilisateurRepository.GetAllUtilisateursAsync();
                    if (autresAdminsActifs.Count(u => u.Role == "Admin" && u.EstActif && u.IDUtilisateur != userToToggle.IDUtilisateur) == 0)
                    {
                        StatusMessage = "Impossible de désactiver le seul administrateur actif.";
                        return;
                    }
                }

                userToToggle.EstActif = !userToToggle.EstActif;
                await _utilisateurRepository.UpdateUtilisateurAsync(userToToggle); // UpdateUtilisateurAsync doit pouvoir juste changer EstActif

                StatusMessage = $"Utilisateur '{userToToggle.Login}' {(userToToggle.EstActif ? "activé" : "désactivé")}.";
                await LoadUtilisateursAsync(); // Recharger pour voir le changement
                ClearInputFieldsAndSelection();
            }
            catch (Exception ex) { StatusMessage = $"Erreur activation/désactivation: {ex.Message}"; }
        }
        
        private void CancelCurrentEdit()
        {
            IsEditing = false;
            SelectedUtilisateur = null; // Désélectionne et vide le formulaire
            StatusMessage = "Opération annulée.";
        }
        
        private void ClearInputFieldsAndSelection()
        {
            ClearInputFields();
            SelectedUtilisateur = null;
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}