using Microsoft.EntityFrameworkCore;
using TransactionsAPI.Model;

namespace TransactionsAPI.Data
{
    public class TransactionsDbContext: DbContext
    {

        public TransactionsDbContext(DbContextOptions<TransactionsDbContext> options)
            : base(options)
        {
        }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

        }
    }
}
