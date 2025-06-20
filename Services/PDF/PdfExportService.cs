
using System.IO;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using centre_soutien.Models;
using System.Linq;

namespace centre_soutien.Services.PDF
{
    public class PdfExportService : IPdfExportService
    {
        private static readonly string _centreName = "Centre de Soutien Scolaire";

        // Configuration du resolver de polices
        static PdfExportService()
        {
            GlobalFontSettings.FontResolver = new CustomFontResolver();
        }

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

                    // Utiliser des polices avec le nom résolu
                    var titleFont = new XFont("MyFont", 16, XFontStyleEx.Bold);
                    var headerFont = new XFont("MyFont", 12, XFontStyleEx.Bold);
                    var textFont = new XFont("MyFont", 10, XFontStyleEx.Regular);

                    double yPosition = 50;

                    // En-tête avec rectangle coloré
                    var headerRect = new XRect(0, 0, page.Width, 80);
                    var brush = new XSolidBrush(XColor.FromArgb(102, 126, 234));
                    graphics.DrawRectangle(brush, headerRect);

                    // Titre du centre en blanc
                    graphics.DrawString(_centreName, titleFont, XBrushes.White, new XPoint(50, 30));
                    graphics.DrawString("LISTE DES ÉTUDIANTS", headerFont, XBrushes.White, new XPoint(50, 55));

                    // Date
                    graphics.DrawString($"Généré le : {DateTime.Now:dd/MM/yyyy à HH:mm}",
                        new XFont("MyFont", 8), XBrushes.White,
                        new XPoint(page.Width - 200, 55));

                    yPosition = 100;

                    // En-têtes du tableau
                    double startX = 50;
                    double colWidth = 110;
                    double rowHeight = 25;

                    string[] headers = { "Nom", "Prénom", "Lycée", "Téléphone", "Inscription" };

                    // Fond des en-têtes
                    var tableHeaderRect = new XRect(startX, yPosition, colWidth * headers.Length, rowHeight);
                    graphics.DrawRectangle(new XSolidBrush(XColor.FromArgb(240, 240, 240)), tableHeaderRect);
                    graphics.DrawRectangle(XPens.Black, tableHeaderRect);

                    // Dessiner les en-têtes
                    for (int i = 0; i < headers.Length; i++)
                    {
                        graphics.DrawString(headers[i], headerFont, XBrushes.Black,
                            new XPoint(startX + (i * colWidth) + 5, yPosition + 15));
                    }

                    yPosition += rowHeight;

                    // Dessiner les données
                    foreach (var etudiant in etudiants)
                    {
                        // Vérifier si on doit créer une nouvelle page
                        if (yPosition > page.Height - 100)
                        {
                            // Numéro de page
                            graphics.DrawString($"Page {document.Pages.Count}",
                                new XFont("MyFont", 8), XBrushes.Gray,
                                new XPoint(page.Width - 100, page.Height - 30));

                            // Nouvelle page
                            graphics.Dispose();
                            page = document.AddPage();
                            graphics = XGraphics.FromPdfPage(page);
                            yPosition = 50;

                            // Redessiner les en-têtes
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
                        string[] data =
                        {
                            etudiant.Nom ?? "",
                            etudiant.Prenom ?? "",
                            etudiant.Lycee ?? "",
                            etudiant.Telephone ?? "",
                            !string.IsNullOrEmpty(etudiant.DateInscriptionSysteme)
                                ? DateTime.Parse(etudiant.DateInscriptionSysteme).ToString("dd/MM/yyyy")
                                : ""
                        };

                        // Dessiner les cellules
                        for (int i = 0; i < data.Length; i++)
                        {
                            var cellRect = new XRect(startX + (i * colWidth), yPosition, colWidth, rowHeight);
                            graphics.DrawRectangle(XPens.Gray, cellRect);

                            // Tronquer le texte si nécessaire
                            string cellText = data[i].Length > 14 ? data[i].Substring(0, 11) + "..." : data[i];

                            graphics.DrawString(cellText, textFont, XBrushes.Black,
                                new XPoint(startX + (i * colWidth) + 5, yPosition + 15));
                        }

                        yPosition += rowHeight;
                    }

                    // Total à la fin
                    yPosition += 15;
                    graphics.DrawString($"Total: {etudiants.Count} étudiant(s)",
                        headerFont, XBrushes.Black, new XPoint(startX, yPosition));

                    // Numéro de page final
                    graphics.DrawString($"Page {document.Pages.Count}",
                        new XFont("MyFont", 8), XBrushes.Gray,
                        new XPoint(page.Width - 100, page.Height - 30));

                    graphics.Dispose();
                    document.Save(filePath);
                    document.Close();
                });

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur export PDF: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        public async Task<bool> ExportEtudiantDetailsAsync(Etudiant etudiant, List<Inscription> inscriptions,
            List<Paiement> paiements, string filePath)
        {
            return await Task.FromResult(true);
        }

        public async Task<bool> ExportPaiementsAsync(List<Paiement> paiements, string filePath)
        {
            return await Task.FromResult(true);
        }

        // MÉTHODE CORRIGÉE : Maintenant dans la classe principale PdfExportService
        public async Task<bool> ExportInscriptionsAsync(List<Inscription> inscriptions, string filePath)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== DÉBUT CRÉATION PDF INSCRIPTIONS ===");
                System.Diagnostics.Debug.WriteLine($"Fichier de destination: {filePath}");
                System.Diagnostics.Debug.WriteLine($"Nombre d'inscriptions: {inscriptions.Count}");

                await Task.Run(() =>
                {
                    System.Diagnostics.Debug.WriteLine("Début de la création du document PDF...");

                    // Créer le document PDF
                    var document = new PdfDocument();
                    document.Info.Title = "Liste des Inscriptions";
                    document.Info.Author = _centreName;
                    document.Info.Creator = "Centre Soutien App";

                    // Créer la première page
                    var page = document.AddPage();
                    var graphics = XGraphics.FromPdfPage(page);

                    System.Diagnostics.Debug.WriteLine("Document PDF créé, création des polices...");

                    // Polices
                    var titleFont = new XFont("MyFont", 16, XFontStyleEx.Bold);
                    var headerFont = new XFont("MyFont", 10, XFontStyleEx.Bold);
                    var textFont = new XFont("MyFont", 8, XFontStyleEx.Regular);

                    double yPosition = 50;

                    System.Diagnostics.Debug.WriteLine("Création de l'en-tête...");

                    // En-tête avec rectangle coloré
                    var headerRect = new XRect(0, 0, page.Width, 80);
                    var brush = new XSolidBrush(XColor.FromArgb(102, 126, 234));
                    graphics.DrawRectangle(brush, headerRect);

                    // Titre
                    graphics.DrawString(_centreName, titleFont, XBrushes.White, new XPoint(50, 30));
                    graphics.DrawString("LISTE DES INSCRIPTIONS", new XFont("MyFont", 12, XFontStyleEx.Bold),
                        XBrushes.White, new XPoint(50, 55));
                    graphics.DrawString($"Généré le : {DateTime.Now:dd/MM/yyyy à HH:mm}",
                        new XFont("MyFont", 8), XBrushes.White,
                        new XPoint(page.Width - 200, 55));

                    yPosition = 100;

                    System.Diagnostics.Debug.WriteLine("Création du tableau...");

                    // En-têtes du tableau
                    double startX = 30;
                    double[] colWidths = { 120, 100, 80, 70, 80, 60, 60 }; // Ajuster les largeurs
                    double rowHeight = 20;

                    string[] headers =
                        { "Étudiant", "Groupe", "Matière", "Prix/mois", "Inscrit le", "Jour éch.", "Statut" };

                    // Fond des en-têtes
                    var tableHeaderRect = new XRect(startX, yPosition, colWidths.Sum(), rowHeight);
                    graphics.DrawRectangle(new XSolidBrush(XColor.FromArgb(240, 240, 240)), tableHeaderRect);
                    graphics.DrawRectangle(XPens.Black, tableHeaderRect);

                    // Dessiner les en-têtes
                    double currentX = startX;
                    for (int i = 0; i < headers.Length; i++)
                    {
                        graphics.DrawString(headers[i], headerFont, XBrushes.Black,
                            new XPoint(currentX + 3, yPosition + 12));
                        currentX += colWidths[i];
                    }

                    yPosition += rowHeight;

                    System.Diagnostics.Debug.WriteLine(
                        $"Début du traitement de {inscriptions.Count} inscriptions...");

                    // Dessiner les données
                    int compteur = 0;
                    foreach (var inscription in inscriptions)
                    {
                        compteur++;
                        if (compteur % 10 == 0)
                        {
                            System.Diagnostics.Debug.WriteLine(
                                $"Traitement inscription {compteur}/{inscriptions.Count}");
                        }

                        // Vérifier si on doit créer une nouvelle page
                        if (yPosition > page.Height - 100)
                        {
                            System.Diagnostics.Debug.WriteLine("Création d'une nouvelle page...");

                            // Numéro de page
                            graphics.DrawString($"Page {document.Pages.Count}",
                                new XFont("MyFont", 8), XBrushes.Gray,
                                new XPoint(page.Width - 100, page.Height - 30));

                            // Nouvelle page
                            graphics.Dispose();
                            page = document.AddPage();
                            graphics = XGraphics.FromPdfPage(page);
                            yPosition = 50;

                            // Redessiner les en-têtes
                            var newHeaderRect = new XRect(startX, yPosition, colWidths.Sum(), rowHeight);
                            graphics.DrawRectangle(new XSolidBrush(XColor.FromArgb(240, 240, 240)), newHeaderRect);
                            graphics.DrawRectangle(XPens.Black, newHeaderRect);

                            currentX = startX;
                            for (int i = 0; i < headers.Length; i++)
                            {
                                graphics.DrawString(headers[i], headerFont, XBrushes.Black,
                                    new XPoint(currentX + 3, yPosition + 12));
                                currentX += colWidths[i];
                            }

                            yPosition += rowHeight;
                        }

                        // Données de la ligne
                        string[] data =
                        {
                            inscription.Etudiant?.NomComplet ?? "N/A",
                            inscription.Groupe?.NomDescriptifGroupe ?? "N/A",
                            inscription.Groupe?.Matiere?.NomMatiere ?? "N/A",
                            $"{inscription.PrixConvenuMensuel:F0} DH",
                            !string.IsNullOrEmpty(inscription.DateInscription)
                                ? DateTime.Parse(inscription.DateInscription).ToString("dd/MM/yy")
                                : "N/A",
                            inscription.JourEcheanceMensuelle.ToString(),
                            inscription.EstActif ? "Actif" : "Inactif"
                        };

                        // Dessiner les cellules
                        currentX = startX;
                        for (int i = 0; i < data.Length; i++)
                        {
                            var cellRect = new XRect(currentX, yPosition, colWidths[i], rowHeight);
                            graphics.DrawRectangle(XPens.LightGray, cellRect);

                            // Tronquer le texte si nécessaire
                            string cellText = data[i];
                            if (cellText.Length > 15) cellText = cellText.Substring(0, 12) + "...";

                            // Couleur spéciale pour le statut
                            var textColor = XBrushes.Black;
                            if (i == 6) // Colonne statut
                            {
                                textColor = inscription.EstActif ? XBrushes.Green : XBrushes.Red;
                            }

                            graphics.DrawString(cellText, textFont, textColor,
                                new XPoint(currentX + 3, yPosition + 12));

                            currentX += colWidths[i];
                        }

                        yPosition += rowHeight;
                    }

                    System.Diagnostics.Debug.WriteLine("Ajout des statistiques finales...");

                    // Statistiques à la fin
                    yPosition += 15;
                    var actives = inscriptions.Count(i => i.EstActif);
                    var inactives = inscriptions.Count(i => !i.EstActif);

                    graphics.DrawString(
                        $"Total: {inscriptions.Count} inscription(s) • Actives: {actives} • Inactives: {inactives}",
                        headerFont, XBrushes.Black, new XPoint(startX, yPosition));

                    // Numéro de page final
                    graphics.DrawString($"Page {document.Pages.Count}",
                        new XFont("MyFont", 8), XBrushes.Gray,
                        new XPoint(page.Width - 100, page.Height - 30));

                    System.Diagnostics.Debug.WriteLine("Sauvegarde du document...");

                    graphics.Dispose();
                    document.Save(filePath);
                    document.Close();

                    System.Diagnostics.Debug.WriteLine("Document sauvegardé et fermé.");
                });

                System.Diagnostics.Debug.WriteLine("=== FIN CRÉATION PDF INSCRIPTIONS ===");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERREUR dans ExportInscriptionsAsync: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        public async Task<bool> ExportStatistiquesGroupeAsync(string groupeNom, Dictionary<string, object> statistiques,
            string filePath)
        {
            return await Task.FromResult(true);
        }
    }

    // Resolver de polices personnalisé - CLASSE SÉPARÉE
    public class CustomFontResolver : IFontResolver
    {
        public string DefaultFontName => "MyFont";

        public byte[] GetFont(string faceName)
        {
            try
            {
                string fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts),
                    "arial.ttf");

                if (File.Exists(fontPath))
                {
                    return File.ReadAllBytes(fontPath);
                }

                fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "calibri.ttf");
                if (File.Exists(fontPath))
                {
                    return File.ReadAllBytes(fontPath);
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            return new FontResolverInfo("MyFont");
        }
    }
}