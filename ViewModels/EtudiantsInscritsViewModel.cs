// Dans ViewModels/EtudiantsInscritsViewModel.cs - VERSION MISE À JOUR
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
        private readonly GroupeRepository _groupeRepository;
        private readonly PaiementRepository _paiementRepository; // AJOUT
        private readonly int _groupeId; // AJOUT

        private Groupe? _currentGroupe;
        public Groupe? CurrentGroupe
        {
            get => _currentGroupe;
            private set { _currentGroupe = value; OnPropertyChanged(); }
        }
        
        // MODIFICATION : Remplacer par la nouvelle collection
        private ObservableCollection<EtudiantAvecStatutPaiement> _etudiantsAvecStatut;
        public ObservableCollection<EtudiantAvecStatutPaiement> EtudiantsAvecStatut
        {
            get => _etudiantsAvecStatut;
            set { _etudiantsAvecStatut = value; OnPropertyChanged(); }
        }

        private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        // Propriétés pour les statistiques d'affichage
        public int NombreEtudiantsPayes => EtudiantsAvecStatut?.Count(e => e.StatutPaiementGroupe == "Payé") ?? 0;
        public int NombreEtudiantsEnRetard => EtudiantsAvecStatut?.Count(e => e.StatutPaiementGroupe == "En retard") ?? 0;
        public int NombreEtudiantsPartiels => EtudiantsAvecStatut?.Count(e => e.StatutPaiementGroupe == "Partiel") ?? 0;

        // Constructeur mis à jour
        public EtudiantsInscritsViewModel(int groupeId, 
                                          InscriptionRepository inscriptionRepo, 
                                          GroupeRepository? groupeRepo = null)
        {
            _groupeId = groupeId;
            _inscriptionRepository = inscriptionRepo;
            _groupeRepository = groupeRepo;
            _paiementRepository = new PaiementRepository(); // AJOUT

            EtudiantsAvecStatut = new ObservableCollection<EtudiantAvecStatutPaiement>();
            _ = LoadEtudiantsInscritsAvecStatutAsync();
        }

        private async Task LoadEtudiantsInscritsAvecStatutAsync()
        {
            StatusMessage = "Chargement des étudiants inscrits et de leur statut de paiement...";
            try
            {
                // Charger les détails du groupe
                if (_groupeRepository != null)
                {
                    CurrentGroupe = await _groupeRepository.GetGroupeByIdAsync(_groupeId);
                }

                // Charger les inscriptions actives pour ce groupe
                var inscriptions = await _inscriptionRepository.GetActiveInscriptionsForGroupeAsync(_groupeId);
                
                var etudiantsAvecStatut = new List<EtudiantAvecStatutPaiement>();

                // Pour chaque inscription, calculer le statut de paiement
                foreach (var inscription in inscriptions)
                {
                    if (inscription.Etudiant == null) continue;

                    var etudiantAvecStatut = new EtudiantAvecStatutPaiement
                    {
                        Etudiant = inscription.Etudiant,
                        Inscription = inscription
                    };

                    // Calculer le statut de paiement pour le mois courant
                    var statutPaiement = await CalculerStatutPaiementMoisCourant(inscription);
                    etudiantAvecStatut.UpdateStatutPaiement(statutPaiement);

                    etudiantsAvecStatut.Add(etudiantAvecStatut);
                }

                // Trier par nom puis prénom
                var etudiantsTriés = etudiantsAvecStatut
                    .OrderBy(e => e.Etudiant.Nom)
                    .ThenBy(e => e.Etudiant.Prenom)
                    .ToList();

                EtudiantsAvecStatut = new ObservableCollection<EtudiantAvecStatutPaiement>(etudiantsTriés);
                
                // Mettre à jour les statistiques
                OnPropertyChanged(nameof(NombreEtudiantsPayes));
                OnPropertyChanged(nameof(NombreEtudiantsEnRetard));
                OnPropertyChanged(nameof(NombreEtudiantsPartiels));

                string nomGroupe = CurrentGroupe?.NomDescriptifGroupe ?? $"Groupe ID {_groupeId}";
                StatusMessage = $"{EtudiantsAvecStatut.Count} étudiant(s) inscrit(s) au groupe '{nomGroupe}'. " +
                               $"Payés: {NombreEtudiantsPayes}, En retard: {NombreEtudiantsEnRetard}, Partiels: {NombreEtudiantsPartiels}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur lors du chargement des étudiants inscrits : {ex.Message}";
            }
        }

        /// <summary>
        /// Calcule le statut de paiement d'une inscription pour le mois courant
        /// </summary>
        private async Task<StatutPaiementMensuel?> CalculerStatutPaiementMoisCourant(Inscription inscription)
        {
            try
            {
                var anneeActuelle = DateTime.Now.Year;
                var moisActuel = DateTime.Now.Month;
                
                // Utiliser la méthode existante pour obtenir le statut du mois courant
                var statutsMensuelsPourInscription = await _paiementRepository.GetStatutPaiementParMoisEtMatiereAsync(
                    inscription.IDEtudiant, 
                    inscription.IDInscription, 
                    anneeActuelle);

                // Retourner seulement le statut du mois courant
                return statutsMensuelsPourInscription.FirstOrDefault(s => s.Mois == moisActuel);
            }
            catch (Exception ex)
            {
                // En cas d'erreur, retourner null (sera géré comme "Non défini")
                System.Diagnostics.Debug.WriteLine($"Erreur calcul statut paiement: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Méthode pour actualiser les données (peut être appelée depuis l'interface)
        /// </summary>
        public async Task RefreshDataAsync()
        {
            await LoadEtudiantsInscritsAvecStatutAsync();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}