// Mise à jour de ton modèle Inscription.cs

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.ComponentModel.DataAnnotations.Schema; // IMPORTANT : Ajouter cette ligne


namespace centre_soutien.Models
{
    public class Inscription : INotifyPropertyChanged
    {
        public int IDInscription { get; set; }
        public int IDEtudiant { get; set; }
        public int IDGroupe { get; set; }
        public string DateInscription { get; set; } = DateTime.Now.ToString("yyyy-MM-dd");   
        public double PrixConvenuMensuel { get; set; }
        public int JourEcheanceMensuelle { get; set; }
        
        public bool EstActif { get; set; } = true; // Par défaut, une nouvelle inscription est active
        public string? DateDesinscription { get; set; } // Nullable, format "YYYY-MM-DD"

        // Navigation vers les entités "parentes" et "enfants"
        public virtual Etudiant? Etudiant { get; set; }
        public virtual Groupe? Groupe { get; set; }
        public virtual ICollection<DetailPaiement> DetailsPaiements { get; set; } = new List<DetailPaiement>();

        // Propriété pour gérer la sélection dans l'interface (non persistée en base)
        [NotMapped] // IMPORTANT : Cet attribut dit à EF de ne pas mapper cette propriété
        private bool _isSelected;
        [NotMapped] // IMPORTANT : Cet attribut dit à EF de ne pas mapper cette propriété
        public bool IsSelected
        {
            get => _isSelected;
            set 
            { 
                _isSelected = value; 
                OnPropertyChanged();
            }
        }

        public Inscription()
        {
            // DateInscription est initialisée ci-dessus.
            // EstActif est initialisé à true.
        }

        // Implémentation de INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}