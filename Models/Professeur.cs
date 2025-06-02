using System.Collections.Generic;

namespace centre_soutien.Models
{
    public class Professeur
    {
        public int IDProfesseur { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        public string? Telephone { get; set; } // Nullable si le téléphone est optionnel
        public string? Notes { get; set; }     // Nullable
        public bool EstArchive { get; set; }
        public string NomComplet => $"{Prenom} {Nom}"; // Ou $"{Nom} {Prenom}" selon ta préférence

        // Navigation: Un professeur peut enseigner plusieurs matières (via ProfesseurMatieres) et animer plusieurs groupes
        public virtual ICollection<ProfesseurMatiere> ProfesseurMatieres { get; set; } = new List<ProfesseurMatiere>();
        public virtual ICollection<Groupe> Groupes { get; set; } = new List<Groupe>();
    }
}