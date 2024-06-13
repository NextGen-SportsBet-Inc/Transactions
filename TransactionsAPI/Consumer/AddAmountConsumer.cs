using MassTransit;
using Shared.Messages;
using TransactionsAPI.Model;
using TransactionsAPI.Repositories;

namespace TransactionsAPI.Consumer
{
    public class AddAmountConsumer(ITransactionsRepository transactionsRepository, ILogger<AddAmountConsumer> logger) : IConsumer<AddAmountRequest>
    {
        private readonly ITransactionsRepository _transactionsRepository = transactionsRepository;
        private readonly ILogger _logger = logger;

        public async Task Consume(ConsumeContext<AddAmountRequest> context)
        {
            double? amountToAdd = context.Message.AmountToAdd;
            _logger.LogInformation("Received request to add currency to user.");

            if (context.Message.UserId == null)
            {
                _logger.LogWarning("Couldn't find user ID in received message.");
                return;
            }

            Account? account = await _transactionsRepository.GetAccountInfoByUserId(context.Message.UserId);

            if (account == null)
            {
                _logger.LogInformation("No account found. Creating a new one.");
                account = await _transactionsRepository.CreateNewAccount(context.Message.UserId);

                Transaction newTransaction = new()
                {
                    TransactionType = TransactionType.CREDIT,
                    Amount = amountToAdd ?? 0,
                    MadeAt = DateTime.UtcNow,
                    Account = account
                };

                account.Transactions.Add(newTransaction);
                account.CurrentAmount = amountToAdd ?? 0.0;
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
            _logger.LogInformation("Updated account for user {userId}.", context.Message.UserId);
            await _transactionsRepository.UpdateAcount(account);
        }
    }
}
