// Dans ViewModels/EtudiantsInscritsViewModel.cs
using centre_soutien.Models;
using centre_soutien.DataAccess;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System; // Pour Exception
using System.Linq; // Pour OrderBy

namespace centre_soutien.ViewModels
{
    public class EtudiantsInscritsViewModel : INotifyPropertyChanged
    {
        private readonly InscriptionRepository _inscriptionRepository;
        private readonly GroupeRepository _groupeRepository; // Optionnel: pour charger le nom du groupe

        private Groupe? _currentGroupe;
        public Groupe? CurrentGroupe
        {
            get => _currentGroupe;
            private set { _currentGroupe = value; OnPropertyChanged(); }
        }
        
        private ObservableCollection<Etudiant> _etudiantsInscrits;
        public ObservableCollection<Etudiant> EtudiantsInscrits
        {
            get => _etudiantsInscrits;
            set { _etudiantsInscrits = value; OnPropertyChanged(); }
        }

        private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        // Constructeur prenant l'ID du groupe et les repositories nécessaires
        public EtudiantsInscritsViewModel(int groupeId, 
                                          InscriptionRepository inscriptionRepo, 
                                          GroupeRepository? groupeRepo = null) // groupeRepo est optionnel
        {
            _inscriptionRepository = inscriptionRepo;
            _groupeRepository = groupeRepo; // Peut être null

            EtudiantsInscrits = new ObservableCollection<Etudiant>();
            _ = LoadEtudiantsInscritsAsync(groupeId);
        }

        private async Task LoadEtudiantsInscritsAsync(int groupeId)
        {
            StatusMessage = "Chargement des étudiants inscrits...";
            try
            {
                if (_groupeRepository != null) // Optionnel: Charger les détails du groupe pour affichage
                {
                    CurrentGroupe = await _groupeRepository.GetGroupeByIdAsync(groupeId);
                }

                var inscriptions = await _inscriptionRepository.GetActiveInscriptionsForGroupeAsync(groupeId);
                var etudiants = inscriptions.Select(i => i.Etudiant)
                                            .Where(e => e != null) // S'assurer que l'étudiant n'est pas null
                                            .OrderBy(e => e!.Nom)   // Utiliser ! si Etudiant est censé être toujours là
                                            .ThenBy(e => e!.Prenom)
                                            .ToList();
                
                EtudiantsInscrits = new ObservableCollection<Etudiant>(etudiants!); // ! pour dire au compilo que etudiants ne sera pas null
                
                string nomGroupe = CurrentGroupe?.NomDescriptifGroupe ?? $"Groupe ID {groupeId}";
                StatusMessage = $"{EtudiantsInscrits.Count} étudiant(s) inscrit(s) au groupe '{nomGroupe}'.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur lors du chargement des étudiants inscrits : {ex.Message}";
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}