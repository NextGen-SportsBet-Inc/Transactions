using System.ComponentModel.DataAnnotations;

namespace TransactionsAPI.Model
{
    public class Account
    {
        [Key]
        public int AccountId { get; set; } 

        public required string UserId { get; set; }

        public required float CurrentAmount { get; set; } = 0;

        public required DateTime CreatedAt { get; set; }

        public Account()
        {
            Transactions = [];
        }

        public HashSet<Transaction> Transactions { get; set; }

    }
}
