using MassTransit;
using Shared.Messages;
using TransactionsAPI.Model;
using TransactionsAPI.Repositories;

namespace SportBetInc.Consumer
{
    public class RemoveAmountConsumer(ITransactionsRepository transactionsRepository) : IConsumer<RemoveAmountRequest>
    {
        private readonly ITransactionsRepository _transactionsRepository = transactionsRepository;

        public async Task Consume(ConsumeContext<RemoveAmountRequest> context)
        {

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
                await context.RespondAsync(response);
                return;
            }
            
            if ((account.CurrentAmount - context.Message.AmountToRemove) < 0)
            {
                response.Success = false;
                response.ErrorMessage = "User doesn't have enough currency to bet.";
                await context.RespondAsync(response);
                return;
            }

            await _transactionsRepository.PerformBalanceDeduction(account, amountToRemove ?? 0);

            response.Success = true;

            await context.RespondAsync(response);

            return;
        }
    }

    
}
