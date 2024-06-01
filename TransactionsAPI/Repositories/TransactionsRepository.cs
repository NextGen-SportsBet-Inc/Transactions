﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using TransactionsAPI.CustomExceptions;
using TransactionsAPI.Data;
using TransactionsAPI.Model;
using TransactionsAPI.Model.DTOs;

namespace TransactionsAPI.Repositories
{
    public class TransactionsRepository(TransactionsDbContext transactionsContext) : ITransactionsRepository
    {
        private readonly TransactionsDbContext _transactionsContext = transactionsContext;

        public virtual async Task<Account?> GetAccountInfoByUserId(String userId)
        {
            return await GetAccountByUserId(userId);
        }

        public virtual async Task<Account> CreateNewAccount(String userId)
        {
            Account newAccount = new()
            {
                CurrentAmount = 0,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                Transactions = []
            };

            var account = await _transactionsContext.Accounts.AddAsync(newAccount);
            await _transactionsContext.SaveChangesAsync();

            return account.Entity;

        }

        public virtual async Task<Transaction?> AddTransactionToAccount(int accountId, TransactionFormDTO transactionForm)
        {
            var account = await GetAccountByAccountId(accountId);
            
            if (account == null) { return null; }


            Transaction newTransaction = new ()
            {
                TransactionType = transactionForm.TransactionType,
                Amount = transactionForm.Amount,
                MadeAt = DateTime.UtcNow,
                Account = account
            };

            account.Transactions.Add(newTransaction);
            await _transactionsContext.SaveChangesAsync();
            return newTransaction;
        }

        public virtual async Task<Account> UpdateAccountBalance(Account account, TransactionFormDTO transactionForm)
        {
            float newAmount = account.CurrentAmount;
            
            if (transactionForm.TransactionType == TransactionType.CREDIT) 
            {
                Console.WriteLine("in credit");
                newAmount += transactionForm.Amount;
            }
            else if (transactionForm.TransactionType == TransactionType.DEBIT) 
            {
                Console.WriteLine("in credit");
                newAmount -= transactionForm.Amount;
                if (newAmount < 0) 
                {
                    throw new NotEnoughCurrencyException("Can't make the operation - there is not enough currency to debit.");
                };
            };

            Console.WriteLine("New amount: " + newAmount.ToString());

            account.CurrentAmount = newAmount;

            Console.WriteLine("Amount in account: " + account.CurrentAmount.ToString());

            _transactionsContext.Update(account);
            await _transactionsContext.SaveChangesAsync();

            return account;

        }

        public virtual async Task RollbackTransaction(Transaction transaction)
        {
            _transactionsContext.Remove(transaction);
            await _transactionsContext.SaveChangesAsync();
        } 


        //auxiliar functions to reduce code duplication
        private async Task<Account?> GetAccountByAccountId(int accountId)
        {
            var account = await _transactionsContext.Accounts
                                        .Include(a => a.Transactions)
                                        .FirstOrDefaultAsync(a => a.AccountId == accountId);

            return account;
        }

        private async Task<Account?> GetAccountByUserId(string userId)
        {
            var account = await _transactionsContext.Accounts
                                        .Include(a => a.Transactions)
                                        .FirstOrDefaultAsync(a => a.UserId == userId);

            return account;
        }
    }
}
