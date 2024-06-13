using MassTransit;
using Shared.Messages;
using TransactionsAPI.Model;
using TransactionsAPI.Repositories;

namespace SportBetInc.Consumer
{
    public class RemoveAmountConsumer(ITransactionsRepository transactionsRepository, ILogger<RemoveAmountConsumer> logger) : IConsumer<RemoveAmountRequest>
    {
        private readonly ITransactionsRepository _transactionsRepository = transactionsRepository;
        private readonly ILogger<RemoveAmountConsumer> _logger = logger;

        public async Task Consume(ConsumeContext<RemoveAmountRequest> context)
        {
            _logger.LogInformation("Received a remove amount request");
            float? amountToRemove = context.Message.AmountToRemove;

            var response = new RemoveAmountResponse
            {
                Success = false
            };

            if (context.Message.AmountToRemove == 0) // no amount to remove so success ig??
            {
                response.Success = true;
            }

            if (context.Message.UserId == null || amountToRemove == null)
            {
                _logger.LogWarning("Couldn't find user Id in message.");
                response.ErrorMessage = "User Id or amount to remove are null";
                await context.RespondAsync(response);
                return;
            }

            //get the amount
            Account? account = await _transactionsRepository.GetAccountInfoByUserId(context.Message.UserId);

            if (account == null)
            {
                response.Success = false;
                response.ErrorMessage = "User has no currency.";
                _logger.LogWarning("User {userId} has no currency to debit from.", context.Message.UserId);
                await context.RespondAsync(response);
                return;
            }
            
            if ((account.CurrentAmount - context.Message.AmountToRemove) < 0)
            {
                response.Success = false;
                _logger.LogWarning("User {userId} has not enough currency to debit from.", context.Message.UserId);
                response.ErrorMessage = "User doesn't have enough currency to bet.";
                await context.RespondAsync(response);
                return;
            }

            await _transactionsRepository.PerformBalanceDeduction(account, amountToRemove ?? 0);

            response.Success = true;

            _logger.LogInformation("Sent success response from debit amount request.");
            await context.RespondAsync(response);

            return;
        }
    }

    
}
