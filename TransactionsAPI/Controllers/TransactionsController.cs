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
    public class TransactionsController(ITransactionsRepository transactionsContext, TransactionService transactionService, ILogger<TransactionsController> logger) : ControllerBase
    {
        private readonly ITransactionsRepository _transactionsContext = transactionsContext;
        private readonly TransactionService _service = transactionService;
        private readonly ILogger _logger = logger;


        [Authorize]
        [HttpPost("/makeTransaction")]
        public async Task<IActionResult> MakeTransaction([FromBody] TransactionFormDTO transactionForm)
        {
            //Verify if user exists through message queue
            Claim? userClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userClaim == null || userClaim.Value == null) {
                _logger.LogWarning("Can't find ID in user token");
                return NotFound(new { message = "Can't find ID in user token." }); 
            }

            bool userExists = await _service.CheckIfUserExists(userClaim.Value);

            if (!userExists) 
            {
                _logger.LogWarning("User with id {} is not registered.", userClaim.Value);
                return Conflict(new { message = "User is not registered. Please create an account." }); 
            }

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
                _logger.LogInformation("User {userID} made a transaction", userClaim.Value);
                return Ok(new { message = "Transaction made sucessfully.", accountDetails = new_account });

            }
            catch (NotEnoughCurrencyException ex)
            {
                _logger.LogWarning("Not enough currency to debt.");
                await _transactionsContext.RollbackTransaction(transaction);
                return Conflict(new { message = ex.Message });
            }

        }

        [Authorize]
        [HttpGet("/getCurrencyAmmount")]
        public async Task<IActionResult> GetCurrencyAmmount()
        {
            Claim? userClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userClaim == null || userClaim.Value == null) 
            {
                _logger.LogWarning("Can't find ID in user token");
                return NotFound(new { message = "Can't find ID in user token." }); 
            }

            bool userExists = await _service.CheckIfUserExists(userClaim.Value);

            if (!userExists) 
            {
                _logger.LogWarning("Can't find user with ID {userId}", userClaim.Value);
                return Conflict(new { message = "User is not registered. Please create an account." }); 
            }

            Account? account = await _transactionsContext.GetAccountInfoByUserId(userClaim.Value);

            account ??= await _transactionsContext.CreateNewAccount(userClaim.Value);

            _logger.LogInformation("User has obtained the amount of currency they possess.");
            return Ok(new { Account = account });

        }

    }
}
