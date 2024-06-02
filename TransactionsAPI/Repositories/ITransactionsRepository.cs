using TransactionsAPI.Model;
using TransactionsAPI.Model.DTOs;

namespace TransactionsAPI.Repositories
{
    public interface ITransactionsRepository
    {
        Task<Account?> GetAccountInfoByUserId(String userId);

        Task<Account> CreateNewAccount(String userId);

        Task<Transaction?> AddTransactionToAccount(int accountId, TransactionFormDTO transactionForm);

        Task<Account> UpdateAccountBalance(Account account, TransactionFormDTO transactionForm);

        Task RollbackTransaction(Transaction transaction);

        Task UpdateAcount(Account account);
    }
}
