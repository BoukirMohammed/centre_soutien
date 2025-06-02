using System.Collections.Generic;

namespace centre_soutien.Models
{
    public class Salle
    {
        public int IDSalle { get; set; }
        public string NomOuNumeroSalle { get; set; }= string.Empty;
        public int? Capacite { get; set; }
        public string Description { get; set; }= string.Empty;
        public bool EstArchivee { get; set; }

        // Navigation: Une salle peut avoir plusieurs séances planifiées
        public virtual ICollection<SeancePlanning> SeancesPlanning { get; set; } = new List<SeancePlanning>();
    }
}