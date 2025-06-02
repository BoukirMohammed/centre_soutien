namespace centre_soutien.Models
{
    public class DetailPaiement
    {
        public int IDDetailPaiement { get; set; }
        public int IDPaiement { get; set; }
        public int IDInscription { get; set; }
        public string AnneeMoisConcerne { get; set; } = string.Empty;
        public double MontantPayePourEcheance { get; set; }

        // Navigation vers les entités "parentes"
        public virtual Paiement Paiement { get; set; }
        public virtual Inscription Inscription { get; set; }
    }
}