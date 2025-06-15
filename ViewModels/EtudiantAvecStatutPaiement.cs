using System.ComponentModel;
using System.Runtime.CompilerServices;
using centre_soutien.Models;

namespace centre_soutien.ViewModels
{
    /// <summary>
    /// Classe wrapper pour afficher un étudiant avec son statut de paiement spécifique au groupe
    /// </summary>
    public class EtudiantAvecStatutPaiement : INotifyPropertyChanged
    {
        public Etudiant Etudiant { get; set; } = new Etudiant();
        public Inscription? Inscription { get; set; } // L'inscription de cet étudiant dans le groupe
        
        private string _statutPaiementGroupe = "Non défini";
        public string StatutPaiementGroupe
        {
            get => _statutPaiementGroupe;
            set { _statutPaiementGroupe = value; OnPropertyChanged(); }
        }

        private string _couleurFond = "#FFFFFF";
        public string CouleurFond
        {
            get => _couleurFond;
            set { _couleurFond = value; OnPropertyChanged(); }
        }

        private string _couleurTexte = "#000000";
        public string CouleurTexte
        {
            get => _couleurTexte;
            set { _couleurTexte = value; OnPropertyChanged(); }
        }

        private string _iconeStatut = "⚪";
        public string IconeStatut
        {
            get => _iconeStatut;
            set { _iconeStatut = value; OnPropertyChanged(); }
        }

        private string _tooltipStatut = "";
        public string TooltipStatut
        {
            get => _tooltipStatut;
            set { _tooltipStatut = value; OnPropertyChanged(); }
        }

        // Propriétés de l'étudiant pour l'affichage direct
        public string NomComplet => Etudiant?.NomComplet ?? "Inconnu";
        public string Telephone => Etudiant?.Telephone ?? "Non renseigné";
        public string Lycee => Etudiant?.Lycee ?? "Non renseigné";

        /// <summary>
        /// Met à jour le statut de paiement basé sur les données du mois courant
        /// </summary>
        /// <param name="statutMensuel">Le statut de paiement pour le mois courant</param>
        public void UpdateStatutPaiement(StatutPaiementMensuel? statutMensuel)
        {
            if (statutMensuel == null)
            {
                // Aucune donnée de paiement disponible
                StatutPaiementGroupe = "Non défini";
                CouleurFond = "#FFFFFF";
                CouleurTexte = "#666666";
                IconeStatut = "⚪";
                TooltipStatut = "Aucune information de paiement disponible";
                return;
            }

            // Mettre à jour selon l'état du paiement
            if (statutMensuel.EstCompletementPaye)
            {
                // Payé - Vert clair
                StatutPaiementGroupe = "Payé";
                CouleurFond = "#e6ffed";
                CouleurTexte = "#2d5a3d";
                IconeStatut = "✅";
                TooltipStatut = $"Payé : {statutMensuel.MontantPayeFormate} / {statutMensuel.MontantDuFormate}";
            }
            else if (statutMensuel.EstPartiellementPaye)
            {
                // Partiellement payé - Orange clair
                StatutPaiementGroupe = "Partiel";
                CouleurFond = "#fff3cd";
                CouleurTexte = "#856404";
                IconeStatut = "⚠️";
                TooltipStatut = $"Partiel : {statutMensuel.MontantPayeFormate} / {statutMensuel.MontantDuFormate}";
            }
            else if (statutMensuel.EstEnRetard)
            {
                // En retard - Rouge clair
                StatutPaiementGroupe = "En retard";
                CouleurFond = "#ffe6e6";
                CouleurTexte = "#721c24";
                IconeStatut = "❌";
                TooltipStatut = $"En retard : {statutMensuel.MontantRestantFormate} restant à payer";
            }
            else if (statutMensuel.MontantDu > 0)
            {
                // À payer (mois courant, pas encore en retard)
                StatutPaiementGroupe = "À payer";
                CouleurFond = "#e3f2fd";
                CouleurTexte = "#1565c0";
                IconeStatut = "🔵";
                TooltipStatut = $"À payer : {statutMensuel.MontantDuFormate}";
            }
            else
            {
                // Aucun montant dû
                StatutPaiementGroupe = "Aucun montant dû";
                CouleurFond = "#f5f5f5";
                CouleurTexte = "#666666";
                IconeStatut = "➖";
                TooltipStatut = "Aucun montant dû pour ce mois";
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}