namespace AMSPM_BackEnd.Models
{
    public class QuoteInputModel
    {
        public string CustomerId { get; set; }
        public string ProjectName { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public DateTime ValidUntil { get; set; }
    }
}
