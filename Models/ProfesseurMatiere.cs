namespace centre_soutien.Models
{
    public class ProfesseurMatiere
    {
        public int IDProfesseurMatiere { get; set; }
        public int IDProfesseur { get; set; }
        public int IDMatiere { get; set; }
        public double PourcentageRemuneration { get; set; }

        // Navigation vers les entités "parentes"
        public virtual Professeur Professeur { get; set; }
        public virtual Matiere Matiere { get; set; }
    }
}