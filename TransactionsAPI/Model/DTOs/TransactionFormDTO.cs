namespace TransactionsAPI.Model.DTOs
{
    public class TransactionFormDTO
    {
        public required float Amount { get; set; }

        public required TransactionType TransactionType { get; set; }
    }
}
