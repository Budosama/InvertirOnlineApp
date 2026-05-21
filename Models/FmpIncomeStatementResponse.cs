namespace InvertirOnlineApp.Models
{
    public class FmpIncomeStatementResponse
    {
        public string? date { get; set; }

        public decimal? revenue { get; set; }

        public decimal? netIncome { get; set; }

        public decimal? eps { get; set; }
    }
}