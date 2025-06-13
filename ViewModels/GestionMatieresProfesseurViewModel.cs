using centre_soutien.Models;
using centre_soutien.DataAccess;
using centre_soutien.Commands; // Pour RelayCommand
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using centre_soutien.Services;

namespace centre_soutien.ViewModels
{
    public class MatiereProfesseurDisplayItem : INotifyPropertyChanged
    {
        public int IDMatiere { get; set; }
        public string NomMatiere { get; set; } = string.Empty;


        private bool _estEnseignee;
        public bool EstEnseignee
        {
            get => _estEnseignee;
            set 
            { 
                if (_estEnseignee != value)
                {
                    _estEnseignee = value; 
                    OnPropertyChanged(); 
                    OnPropertyChanged(nameof(PeutSaisirPourcentage));
                    if (!_estEnseignee) // Si décoché, remettre le pourcentage à 0 ou une valeur par défaut
                    {
                        PourcentageRemuneration = 0;
                    }
                }
            }
        }

        private double _pourcentageRemuneration;
        public double PourcentageRemuneration
        {
            get => _pourcentageRemuneration;
            set 
            { 
                if (_pourcentageRemuneration != value)
                {
                    _pourcentageRemuneration = value; 
                    OnPropertyChanged(); 
                }
            }
        }
        
        public bool PeutSaisirPourcentage => EstEnseignee;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class GestionMatieresProfesseurViewModel : INotifyPropertyChanged
    {
        private readonly ProfesseurRepository _professeurRepository;
        private readonly MatiereRepository _matiereRepository;
        private readonly ProfesseurMatiereRepository _profMatiereRepository;
        private readonly int _professeurId; // Stocker l'ID du professeur concerné

        private Professeur? _currentProfesseur; // Pour afficher le nom du professeur si besoin
        public Professeur? CurrentProfesseur
        {
            get => _currentProfesseur;
            private set { _currentProfesseur = value; OnPropertyChanged(); }
        }

        private ObservableCollection<MatiereProfesseurDisplayItem> _matieresPourProfesseur;
        public ObservableCollection<MatiereProfesseurDisplayItem> MatieresPourProfesseur
        {
            get => _matieresPourProfesseur;
            set { _matieresPourProfesseur = value; OnPropertyChanged(); }
        }

        private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }
        
        public bool ModificationsEnregistrees { get; private set; } = false;

        public ICommand SaveChangesCommand { get; }
        // La commande Annuler sera gérée par la fenêtre (bouton IsCancel=True)

        // Constructeur complet et corrigé
        public GestionMatieresProfesseurViewModel(int professeurId,
                                                  ProfesseurRepository profRepo, 
                                                  MatiereRepository matRepo, 
                                                  ProfesseurMatiereRepository profMatRepo)
        {
            _professeurId = professeurId; // Stocker l'ID du professeur
            _professeurRepository = profRepo;
            _matiereRepository = matRepo;
            _profMatiereRepository = profMatRepo;

            MatieresPourProfesseur = new ObservableCollection<MatiereProfesseurDisplayItem>();

            SaveChangesCommand = new RelayCommand(async param => await SaveChangesAsync(), CanSaveChanges);
            
            _ = LoadDataForProfesseurAsync(); // Charger les données pour le professeur spécifié
        }

        private async Task LoadDataForProfesseurAsync()
        {
            StatusMessage = "Chargement des données du professeur...";
            try
            {
                // Charger les informations du professeur actuel pour affichage (optionnel)
                // Si ProfesseurRepository n'a pas GetByIdAsync, il faudrait l'ajouter.
                // Pour cet exemple, je suppose que tu peux obtenir le nom autrement ou tu l'as déjà.
                // CurrentProfesseur = await _professeurRepository.GetProfesseurByIdAsync(_professeurId); // Si GetProfesseurByIdAsync existe

                var toutesLesMatieres = await _matiereRepository.GetAllMatieresAsync(); // Que les matières actives
                var associationsExistantes = await _profMatiereRepository.GetMatieresForProfesseurAsync(_professeurId);

                var displayItems = new List<MatiereProfesseurDisplayItem>();
                foreach (var matiere in toutesLesMatieres.OrderBy(m => m.NomMatiere)) // Ordonner les matières
                {
                    var association = associationsExistantes.FirstOrDefault(a => a.IDMatiere == matiere.IDMatiere);
                    displayItems.Add(new MatiereProfesseurDisplayItem
                    {
                        IDMatiere = matiere.IDMatiere,
                        NomMatiere = matiere.NomMatiere,
                        EstEnseignee = association != null,
                        PourcentageRemuneration = association?.PourcentageRemuneration ?? 0 
                    });
                }
                MatieresPourProfesseur = new ObservableCollection<MatiereProfesseurDisplayItem>(displayItems);
                
                // Afficher un message avec le nom du professeur si chargé
                // string nomProf = CurrentProfesseur != null ? CurrentProfesseur.NomComplet : $"ID: {_professeurId}";
                // StatusMessage = $"Gérer les matières pour le professeur {nomProf}.";
                StatusMessage = $"Prêt à gérer les matières pour le professeur ID: {_professeurId}.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur lors du chargement des données : {ex.Message}";
            }
        }

        private bool CanSaveChanges(object? parameter)
        {
            // Permettre la sauvegarde s'il y a des matières à gérer
            return MatieresPourProfesseur.Any();
        }

        private async Task SaveChangesAsync()
        {
            if (!CanSaveChanges(null)) return;

            StatusMessage = "Enregistrement des modifications...";
            try
            {
                var nouvellesAssociations = MatieresPourProfesseur
                    .Where(m => m.EstEnseignee) // On ne sauvegarde que celles qui sont cochées comme enseignées
                    .Select(m => new ProfesseurMatiere
                    {
                        IDProfesseur = _professeurId, // Utiliser l'ID stocké
                        IDMatiere = m.IDMatiere,
                        PourcentageRemuneration = m.PourcentageRemuneration
                    }).ToList();

                // Valider les pourcentages avant d'envoyer au repository
                foreach (var association in nouvellesAssociations)
                {
                    if (association.PourcentageRemuneration < 0 || association.PourcentageRemuneration > 100)
                    {
                        var matiereConcernee = MatieresPourProfesseur.First(m => m.IDMatiere == association.IDMatiere);
                        throw new ArgumentOutOfRangeException(nameof(association.PourcentageRemuneration), 
                            $"Le pourcentage pour '{matiereConcernee.NomMatiere}' doit être entre 0 et 100.");
                    }
                }

                await _profMatiereRepository.UpdateAssociationsForProfesseurAsync(_professeurId, nouvellesAssociations);

                StatusMessage = "Modifications enregistrées avec succès.";
                ModificationsEnregistrees = true; // Pour signaler à la vue de se fermer
            }
            catch (ArgumentOutOfRangeException ex) // Pour les pourcentages invalides
            {
                StatusMessage = ex.Message;
                ModificationsEnregistrees = false;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur lors de l'enregistrement: {ex.Message}";
                ModificationsEnregistrees = false;
            }
        }
        
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}