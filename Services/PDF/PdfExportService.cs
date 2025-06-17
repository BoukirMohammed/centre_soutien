// Services/PDF/PdfExportService.cs
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using centre_soutien.Models;

namespace centre_soutien.Services.PDF
{
    public class PdfExportService : IPdfExportService
    {
        private readonly string _centreName = "Centre de Soutien Scolaire";
        private readonly XColor _primaryColor = XColor.FromArgb(102, 126, 234); // #667eea
        private readonly XColor _secondaryColor = XColor.FromArgb(118, 75, 162); // #764ba2

        public async Task<bool> ExportEtudiantsListAsync(List<Etudiant> etudiants, string filePath)
        {
            try
            {
                await Task.Run(() =>
                {
                    // Créer le document PDF
                    var document = new PdfDocument();
                    document.Info.Title = "Liste des Étudiants";
                    document.Info.Author = _centreName;
                    document.Info.Creator = "Centre Soutien App";

                    // Créer la première page
                    var page = document.AddPage();
                    var graphics = XGraphics.FromPdfPage(page);
                    
                    // Définir les polices
                    var titleFont = new XFont("Arial", 16, XFontStyle.Bold);
                    var headerFont = new XFont("Arial", 10, XFontStyle.Bold);
                    var textFont = new XFont("Arial", 9, XFontStyle.Regular);

                    double yPosition = 50;

                    // Dessiner l'en-tête
                    DrawHeader(graphics, titleFont, "LISTE DES ÉTUDIANTS", ref yPosition);
                    
                    // Dessiner le tableau des étudiants
                    DrawEtudiantsTable(graphics, etudiants, headerFont, textFont, ref yPosition, page);

                    graphics.Dispose();
                    document.Save(filePath);
                    document.Close();
                });

                return true;
            }
            catch (Exception ex)
            {
                // Log l'erreur
                System.Diagnostics.Debug.WriteLine($"Erreur export PDF: {ex.Message}");
                return false;
            }
        }

        private void DrawHeader(XGraphics graphics, XFont titleFont, string title, ref double yPosition)
        {
            // Rectangle de couleur en en-tête
            var headerRect = new XRect(0, 0, graphics.PdfPage.Width, 80);
            var brush = new XLinearGradientBrush(headerRect, _primaryColor, _secondaryColor, 
                XLinearGradientMode.Horizontal);
            graphics.DrawRectangle(brush, headerRect);

            // Titre du centre en blanc
            graphics.DrawString(_centreName, titleFont, XBrushes.White, 
                new XPoint(50, 30));
            
            // Sous-titre du rapport
            graphics.DrawString(title, new XFont("Arial", 12, XFontStyle.Bold), 
                XBrushes.White, new XPoint(50, 55));

            // Date et heure
            graphics.DrawString($"Généré le : {DateTime.Now:dd/MM/yyyy à HH:mm}", 
                new XFont("Arial", 8), XBrushes.White, 
                new XPoint(graphics.PdfPage.Width - 200, 55));

            yPosition = 100;
        }

        private void DrawEtudiantsTable(XGraphics graphics, List<Etudiant> etudiants, 
            XFont headerFont, XFont textFont, ref double yPosition, PdfPage page)
        {
            double startX = 50;
            double colWidth = 120;
            double rowHeight = 25;

            // En-têtes du tableau
            string[] headers = { "Nom", "Prénom", "Lycée", "Téléphone", "Date Inscription" };
            
            // Fond des en-têtes
            var headerRect = new XRect(startX, yPosition, colWidth * headers.Length, rowHeight);
            graphics.DrawRectangle(new XSolidBrush(XColor.FromArgb(240, 240, 240)), headerRect);
            graphics.DrawRectangle(XPens.Black, headerRect);

            // Dessiner les en-têtes
            for (int i = 0; i < headers.Length; i++)
            {
                graphics.DrawString(headers[i], headerFont, XBrushes.Black,
                    new XPoint(startX + (i * colWidth) + 5, yPosition + 15));
            }

            yPosition += rowHeight;

            // Compteur pour pagination
            int etudiantCount = 0;

            // Dessiner les données des étudiants
            foreach (var etudiant in etudiants)
            {
                // Vérifier si on doit créer une nouvelle page
                if (yPosition > graphics.PdfPage.Height - 100)
                {
                    // Ajouter le numéro de page
                    graphics.DrawString($"Page {page.Owner.Pages.Count}", 
                        new XFont("Arial", 8), XBrushes.Gray,
                        new XPoint(graphics.PdfPage.Width - 100, graphics.PdfPage.Height - 30));

                    // Créer une nouvelle page
                    var newPage = page.Owner.AddPage();
                    graphics.Dispose();
                    graphics = XGraphics.FromPdfPage(newPage);
                    page = newPage;
                    
                    yPosition = 50;
                    
                    // Redessiner les en-têtes sur la nouvelle page
                    var newHeaderRect = new XRect(startX, yPosition, colWidth * headers.Length, rowHeight);
                    graphics.DrawRectangle(new XSolidBrush(XColor.FromArgb(240, 240, 240)), newHeaderRect);
                    graphics.DrawRectangle(XPens.Black, newHeaderRect);

                    for (int i = 0; i < headers.Length; i++)
                    {
                        graphics.DrawString(headers[i], headerFont, XBrushes.Black,
                            new XPoint(startX + (i * colWidth) + 5, yPosition + 15));
                    }
                    yPosition += rowHeight;
                }

                // Données de la ligne
                string[] data = { 
                    etudiant.Nom ?? "", 
                    etudiant.Prenom ?? "", 
                    etudiant.Lycee ?? "", 
                    etudiant.Telephone ?? "",
                    !string.IsNullOrEmpty(etudiant.DateInscriptionSysteme) ? 
                        DateTime.Parse(etudiant.DateInscriptionSysteme).ToString("dd/MM/yyyy") : ""
                };

                // Dessiner les cellules de la ligne
                for (int i = 0; i < data.Length; i++)
                {
                    var cellRect = new XRect(startX + (i * colWidth), yPosition, colWidth, rowHeight);
                    graphics.DrawRectangle(XPens.Gray, cellRect);
                    
                    // Tronquer le texte si trop long
                    string cellText = data[i].Length > 15 ? data[i].Substring(0, 12) + "..." : data[i];
                    
                    graphics.DrawString(cellText, textFont, XBrushes.Black,
                        new XPoint(startX + (i * colWidth) + 5, yPosition + 15));
                }

                yPosition += rowHeight;
                etudiantCount++;
            }

            // Ajouter le total à la fin
            yPosition += 10;
            graphics.DrawString($"Total: {etudiants.Count} étudiant(s)", 
                new XFont("Arial", 10, XFontStyle.Bold), XBrushes.Black,
                new XPoint(startX, yPosition));

            // Numéro de page final
            graphics.DrawString($"Page {page.Owner.Pages.Count}", 
                new XFont("Arial", 8), XBrushes.Gray,
                new XPoint(graphics.PdfPage.Width - 100, graphics.PdfPage.Height - 30));
        }

        public async Task<bool> ExportEtudiantDetailsAsync(Etudiant etudiant, 
            List<Inscription> inscriptions, List<Paiement> paiements, string filePath)
        {
            try
            {
                await Task.Run(() =>
                {
                    var document = new PdfDocument();
                    document.Info.Title = $"Fiche Étudiant - {etudiant.Prenom} {etudiant.Nom}";
                    
                    var page = document.AddPage();
                    var graphics = XGraphics.FromPdfPage(page);
                    
                    var titleFont = new XFont("Arial", 16, XFontStyle.Bold);
                    var sectionFont = new XFont("Arial", 12, XFontStyle.Bold);
                    var textFont = new XFont("Arial", 10, XFontStyle.Regular);

                    double yPosition = 50;

                    // En-tête
                    DrawHeader(graphics, titleFont, $"FICHE ÉTUDIANT - {etudiant.Prenom} {etudiant.Nom}", ref yPosition);
                    
                    // Section Informations personnelles
                    yPosition += 20;
                    graphics.DrawString("INFORMATIONS PERSONNELLES", sectionFont, 
                        new XSolidBrush(_primaryColor), new XPoint(50, yPosition));
                    yPosition += 25;

                    string[] infos = {
                        $"Nom complet : {etudiant.Prenom} {etudiant.Nom}",
                        $"Date de naissance : {etudiant.DateNaissance ?? "Non renseignée"}",
                        $"Téléphone : {etudiant.Telephone ?? "Non renseigné"}",
                        $"Lycée : {etudiant.Lycee ?? "Non renseigné"}",
                        $"Adresse : {etudiant.Adresse ?? "Non renseignée"}",
                        $"Date d'inscription : {etudiant.DateInscriptionSysteme ?? "Non renseignée"}",
                        $"Notes : {etudiant.Notes ?? "Aucune note"}"
                    };

                    foreach (var info in infos)
                    {
                        graphics.DrawString(info, textFont, XBrushes.Black, 
                            new XPoint(70, yPosition));
                        yPosition += 20;
                    }

                    // Section Inscriptions
                    yPosition += 20;
                    graphics.DrawString("INSCRIPTIONS ACTUELLES", sectionFont, 
                        new XSolidBrush(_primaryColor), new XPoint(50, yPosition));
                    yPosition += 25;

                    if (inscriptions.Any())
                    {
                        foreach (var inscription in inscriptions.Where(i => i.EstActif))
                        {
                            graphics.DrawString($"• {inscription.Groupe?.NomDescriptifGroupe} - " +
                                $"{inscription.Groupe?.Matiere?.NomMatiere} - " +
                                $"{inscription.PrixConvenuMensuel:F2} DH/mois - " +
                                $"Échéance: {inscription.JourEcheanceMensuelle} du mois", 
                                textFont, XBrushes.Black, new XPoint(70, yPosition));
                            yPosition += 18;
                        }
                    }
                    else
                    {
                        graphics.DrawString("Aucune inscription active", textFont, 
                            XBrushes.Gray, new XPoint(70, yPosition));
                    }

                    // Section Historique des paiements (résumé)
                    yPosition += 30;
                    graphics.DrawString("RÉSUMÉ DES PAIEMENTS", sectionFont, 
                        new XSolidBrush(_primaryColor), new XPoint(50, yPosition));
                    yPosition += 25;

                    if (paiements.Any())
                    {
                        var totalPaye = paiements.Sum(p => p.MontantTotalRecuTransaction);
                        var dernierPaiement = paiements.OrderByDescending(p => p.DatePaiement).FirstOrDefault();
                        
                        graphics.DrawString($"• Total payé : {totalPaye:F2} DH", 
                            textFont, XBrushes.Black, new XPoint(70, yPosition));
                        yPosition += 18;
                        
                        graphics.DrawString($"• Nombre de paiements : {paiements.Count}", 
                            textFont, XBrushes.Black, new XPoint(70, yPosition));
                        yPosition += 18;
                        
                        if (dernierPaiement != null)
                        {
                            graphics.DrawString($"• Dernier paiement : {dernierPaiement.DatePaiement} - {dernierPaiement.MontantTotalRecuTransaction:F2} DH", 
                                textFont, XBrushes.Black, new XPoint(70, yPosition));
                        }
                    }
                    else
                    {
                        graphics.DrawString("Aucun paiement enregistré", textFont, 
                            XBrushes.Gray, new XPoint(70, yPosition));
                    }

                    graphics.Dispose();
                    document.Save(filePath);
                    document.Close();
                });

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur export PDF: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ExportPaiementsAsync(List<Paiement> paiements, string filePath)
        {
            // À implémenter - similaire aux étudiants
            return await Task.FromResult(true);
        }

        public async Task<bool> ExportInscriptionsAsync(List<Inscription> inscriptions, string filePath)
        {
            // À implémenter - similaire aux étudiants
            return await Task.FromResult(true);
        }

        public async Task<bool> ExportStatistiquesGroupeAsync(string groupeNom, 
            Dictionary<string, object> statistiques, string filePath)
        {
            // À implémenter - pour les statistiques financières
            return await Task.FromResult(true);
        }
    }
}