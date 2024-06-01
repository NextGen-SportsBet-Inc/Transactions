using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TransactionsAPI.Model
{
    public class Transaction
    {
        [Key]
        public int TransactionId { get; set; }

        public required TransactionType TransactionType { get; set; }

        public required DateTime MadeAt { get; set; }

        public required float Amount { get; set; }

        [JsonIgnore]
        [ForeignKey("AccountId")]
        public Account? Account { get; set; }
    }
}
