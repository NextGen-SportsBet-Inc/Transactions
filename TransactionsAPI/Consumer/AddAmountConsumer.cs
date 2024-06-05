using MassTransit;
using Shared.Messages;
using TransactionsAPI.Model;
using TransactionsAPI.Repositories;

namespace TransactionsAPI.Consumer
{
    public class AddAmountConsumer(ITransactionsRepository transactionsRepository) : IConsumer<AddAmountRequest>
    {
        private readonly ITransactionsRepository _transactionsRepository = transactionsRepository;

        public async Task Consume(ConsumeContext<AddAmountRequest> context)
        {
            float? amountToAdd = context.Message.AmountToAdd;

            if (context.Message.UserId == null)
            {
                return;
            }

            Account? account = await _transactionsRepository.GetAccountInfoByUserId(context.Message.UserId);

            if (account == null)
            {
                account = await _transactionsRepository.CreateNewAccount(context.Message.UserId);

                Transaction newTransaction = new()
                {
                    TransactionType = TransactionType.CREDIT,
                    Amount = amountToAdd ?? 0,
                    MadeAt = DateTime.UtcNow,
                    Account = account
                };

                account.Transactions.Add(newTransaction);
                account.CurrentAmount = amountToAdd ?? 0;
                await _transactionsRepository.UpdateAcount(account);
                return;
            }

            Transaction _newTransaction = new()
            {
                TransactionType = TransactionType.CREDIT,
                Amount = amountToAdd ?? 0,
                MadeAt = DateTime.UtcNow,
                Account = account
            };

            account.Transactions.Add(_newTransaction);
            account.CurrentAmount += amountToAdd ?? 0;
            await _transactionsRepository.UpdateAcount(account);
        }
    }
}
