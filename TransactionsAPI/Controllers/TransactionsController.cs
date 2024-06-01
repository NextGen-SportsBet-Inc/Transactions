using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TransactionsAPI.CustomExceptions;
using TransactionsAPI.Model;
using TransactionsAPI.Model.DTOs;
using TransactionsAPI.Repositories;
using TransactionsAPI.Services;

namespace TransactionsAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TransactionsController(ITransactionsRepository transactionsContext, TransactionService transactionService) : ControllerBase
    {
        private readonly ITransactionsRepository _transactionsContext = transactionsContext;
        private readonly TransactionService _service = transactionService;

        [Authorize]
        [HttpGet("/makeTransaction")]
        public async Task<IActionResult> MakeTransaction([FromBody] TransactionFormDTO transactionForm)
        {
            //Verify if user exists through message queue
            Claim? userClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userClaim == null || userClaim.Value == null) { return NotFound(new { message = "Can't find ID in user token." }); }

            bool userExists = await _service.CheckIfUserExists(userClaim.Value);

            if (!userExists) { return Conflict(new { message = "User is not registered. Please create an account." }); }

            //Verify if account with user exists, if it doesnt, create a new one
            Account? account = await _transactionsContext.GetAccountInfoByUserId(userClaim.Value);

            account ??= await _transactionsContext.CreateNewAccount(userClaim.Value);

            //Add transaction to account
            Transaction? transaction = await _transactionsContext.AddTransactionToAccount(account.AccountId, transactionForm);

            if (transaction == null) { return Conflict(new { message = "Could not add the transaction to account. " }); };

            //Update account balance based on this transaction
            try
            {
                Account new_account = await _transactionsContext.UpdateAccountBalance(account, transactionForm);
                return Ok(new { message = "Transaction made sucessfully.", accountDetails = new_account });

            }
            catch (NotEnoughCurrencyException ex)
            {
                await _transactionsContext.RollbackTransaction(transaction);
                return Conflict(new { message = ex.Message });
            }

        }

        [Authorize]
        [HttpGet("/getCurrencyAmmount")]
        public async Task<IActionResult> GetCurrencyAmmount()
        {
            Claim? userClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userClaim == null || userClaim.Value == null) { return NotFound(new { message = "Can't find ID in user token." }); }

            bool userExists = await _service.CheckIfUserExists(userClaim.Value);

            if (!userExists) { return Conflict(new { message = "User is not registered. Please create an account." }); }

            Account? account = await _transactionsContext.GetAccountInfoByUserId(userClaim.Value);

            account ??= await _transactionsContext.CreateNewAccount(userClaim.Value);

            return Ok(new { Account = account });

        }

    }
}
