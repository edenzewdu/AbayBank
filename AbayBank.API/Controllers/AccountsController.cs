using AbayBank.Application.DTOs;
using AbayBank.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AbayBank.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AccountsController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly ILogger<AccountsController> _logger;

        public AccountsController(
            IAccountService accountService,
            ILogger<AccountsController> logger)
        {
            _accountService = accountService;
            _logger = logger;
        }

        // GET: api/accounts
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllAccounts()
        {
            try
            {
                var accounts = await _accountService.GetAllAccountsAsync();

                return Ok(new ApiResponse<IEnumerable<AccountDto>>(
                    true, "All accounts retrieved", accounts));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all accounts");
                return StatusCode(500, new ApiResponse<string>(
                    false, "Internal server error"));
            }
        }

        // GET: api/accounts/my
        [HttpGet("my")]
        public async Task<IActionResult> GetAccounts()
        {
            try
            {
                var userId = GetCurrentUserId();
                var accounts = await _accountService.GetUserAccountsAsync(userId);
                return Ok(new ApiResponse<IEnumerable<AccountDto>>(true, "Accounts retrieved", accounts));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting accounts");
                return StatusCode(500, new ApiResponse<string>(false, "Internal server error"));
            }
        }

        // GET: api/accounts/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAccount(Guid id)
        {
            try
            {
                var account = await _accountService.GetAccountByIdAsync(id);
                return Ok(new ApiResponse<AccountDto>(true, "Account retrieved", account));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<string>(false, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting account {Id}", id);
                return BadRequest(new ApiResponse<string>(false, ex.Message));
            }
        }

        // POST: api/accounts
        [HttpPost]
        public async Task<IActionResult> CreateAccount([FromBody] CreateAccountRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var account = await _accountService.CreateAccountAsync(request, userId);
                return CreatedAtAction(nameof(GetAccount), new { id = account.Id }, 
                    new ApiResponse<AccountDto>(true, "Account created successfully", account));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating account");
                return BadRequest(new ApiResponse<string>(false, ex.Message));
            }
        }

        // PUT: api/accounts/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAccount(Guid id, [FromBody] UpdateAccountRequest request)
        {
            try
            {
                var account = await _accountService.UpdateAccountAsync(id, request);
                return Ok(new ApiResponse<AccountDto>(true, "Account updated successfully", account));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<string>(false, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating account {Id}", id);
                return BadRequest(new ApiResponse<string>(false, ex.Message));
            }
        }

        // DELETE: api/accounts/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAccount(Guid id)
        {
            try
            {
                await _accountService.DeleteAccountAsync(id);
                return Ok(new ApiResponse<string>(true, "Account deleted successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<string>(false, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting account {Id}", id);
                return BadRequest(new ApiResponse<string>(false, ex.Message));
            }
        }

        // POST: api/accounts/deposit
        [HttpPost("deposit")]
        public async Task<IActionResult> Deposit([FromBody] DepositRequest request)
        {
            try
            {
                var transaction = await _accountService.DepositAsync(request);
                return Ok(new ApiResponse<TransactionDto>(true, "Deposit successful", transaction));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<string>(false, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing deposit");
                return BadRequest(new ApiResponse<string>(false, ex.Message));
            }
        }

        // POST: api/accounts/withdraw
        [HttpPost("withdraw")]
        public async Task<IActionResult> Withdraw([FromBody] WithdrawRequest request)
        {
            try
            {
                var transaction = await _accountService.WithdrawAsync(request);
                return Ok(new ApiResponse<TransactionDto>(true, "Withdrawal successful", transaction));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<string>(false, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing withdrawal");
                return BadRequest(new ApiResponse<string>(false, ex.Message));
            }
        }

        // POST: api/accounts/transfer
        [HttpPost("transfer")]
        public async Task<IActionResult> Transfer([FromBody] TransferRequest request)
        {
            try
            {
                var result = await _accountService.TransferAsync(request);
                return Ok(new ApiResponse<TransferResultDto>(true, "Transfer successful", result));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<string>(false, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing transfer");
                return BadRequest(new ApiResponse<string>(false, ex.Message));
            }
        }

        // GET: api/accounts/{id}/transactions
        [HttpGet("{id}/transactions")]
        public async Task<IActionResult> GetTransactions(Guid id, [FromQuery] TransactionQuery query)
        {
            try
            {
                var transactions = await _accountService.GetAccountTransactionsAsync(id, query);
                return Ok(new ApiResponse<IEnumerable<TransactionDto>>(true, "Transactions retrieved", transactions));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<string>(false, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transactions for account {Id}", id);
                return BadRequest(new ApiResponse<string>(false, ex.Message));
            }
        }

        // PUT: api/accounts/{id}/freeze
        [HttpPut("{id}/freeze")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> FreezeAccount(Guid id, [FromBody] FreezeAccountRequest request)
        {
            try
            {
                var account = await _accountService.FreezeAccountAsync(id, request.Reason);
                return Ok(new ApiResponse<AccountDto>(true, "Account frozen successfully", account));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<string>(false, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error freezing account {Id}", id);
                return BadRequest(new ApiResponse<string>(false, ex.Message));
            }
        }

        // PUT: api/accounts/{id}/unfreeze
        [HttpPut("{id}/unfreeze")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UnfreezeAccount(Guid id)
        {
            try
            {
                var account = await _accountService.UnfreezeAccountAsync(id);
                return Ok(new ApiResponse<AccountDto>(true, "Account unfrozen successfully", account));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<string>(false, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unfreezing account {Id}", id);
                return BadRequest(new ApiResponse<string>(false, ex.Message));
            }
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                throw new UnauthorizedAccessException("User not authenticated");
            
            if (!Guid.TryParse(userIdClaim.Value, out var userId))
                throw new UnauthorizedAccessException("Invalid user ID format");
            
            return userId;
        }
    }
}