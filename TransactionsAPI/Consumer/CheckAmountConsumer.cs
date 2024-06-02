﻿using MassTransit;
using Shared.Messages;
using TransactionsAPI.Model;
using TransactionsAPI.Repositories;

namespace SportBetInc.Consumer
{
    public class CheckAmountConsumer(ITransactionsRepository transactionsRepository) : IConsumer<CheckAmountRequest>
    {
        private readonly ITransactionsRepository _transactionsRepository = transactionsRepository;

        public async Task Consume(ConsumeContext<CheckAmountRequest> context)
        {
            var userId = context.Message.UserId;

            var response = new CheckAmountResponse
            {
                Error = true
            };

            if (userId == null)
            {
                await context.RespondAsync(response);
                return;
            }

            //get the amount
            Account? account = await _transactionsRepository.GetAccountInfoByUserId(userId);

            response.Error = false;

            if (account == null)
            {
                response.Amount = 0;
                await context.RespondAsync(response);
                return;
            }

            response.Amount = account.CurrentAmount;
            await context.RespondAsync(response);

            return;
        }
    }

    
}
