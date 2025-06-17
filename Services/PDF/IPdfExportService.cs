// Services/PDF/IPdfExportService.cs
using centre_soutien.Models;

namespace centre_soutien.Services.PDF
{
    public interface IPdfExportService
    {
        Task<bool> ExportEtudiantsListAsync(List<Etudiant> etudiants, string filePath);
        Task<bool> ExportEtudiantDetailsAsync(Etudiant etudiant, List<Inscription> inscriptions, 
            List<Paiement> paiements, string filePath);
        Task<bool> ExportPaiementsAsync(List<Paiement> paiements, string filePath);
        Task<bool> ExportInscriptionsAsync(List<Inscription> inscriptions, string filePath);
        Task<bool> ExportStatistiquesGroupeAsync(string groupeNom, 
            Dictionary<string, object> statistiques, string filePath);
    }
}

