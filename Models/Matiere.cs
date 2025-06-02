using System.Collections.Generic;

namespace centre_soutien.Models
{
    public class Matiere
    {
        public int IDMatiere { get; set; }
        public string NomMatiere { get; set; }= string.Empty;
        public double PrixStandardMensuel { get; set; }
        public string Description { get; set; }= string.Empty;
        public bool EstArchivee { get; set; }

        // Navigation: Une matière peut être enseignée par plusieurs professeurs (via ProfesseurMatieres) et être dans plusieurs groupes
        public virtual ICollection<ProfesseurMatiere> ProfesseurMatieres { get; set; } = new List<ProfesseurMatiere>();
        public virtual ICollection<Groupe> Groupes { get; set; } = new List<Groupe>();
    }
}