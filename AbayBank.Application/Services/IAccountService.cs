namespace AbayBank.Application.Services;

public interface IAccountService
{
    Task<AccountDto> GetAccountByIdAsync(Guid id);
    Task<AccountDto> GetAccountByNumberAsync(string accountNumber);
    Task<IEnumerable<AccountDto>> GetUserAccountsAsync(Guid userId);
    Task<TransactionDto> DepositAsync(DepositRequest request);
    Task<TransactionDto> WithdrawAsync(WithdrawRequest request);
    Task<TransferResultDto> TransferAsync(TransferRequest request);
    Task<AccountDto> FreezeAccountAsync(Guid accountId, string reason);
    Task<AccountDto> UnfreezeAccountAsync(Guid accountId);
    Task<IEnumerable<TransactionDto>> GetTransactionsAsync(Guid accountId, DateTime? from, DateTime? to);
}

// AccountController.cs
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccountsController : ControllerBase
{
    private readonly IAccountService _accountService;
    
    [HttpPost("deposit")]
    public async Task<IActionResult> Deposit([FromBody] DepositRequest request)
    {
        var result = await _accountService.DepositAsync(request);
        return Ok(result);
    }
    
    [HttpPost("transfer")]
    public async Task<IActionResult> Transfer([FromBody] TransferRequest request)
    {
        var result = await _accountService.TransferAsync(request);
        return Ok(result);
    }
    
    [HttpPut("{id}/freeze")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> FreezeAccount(Guid id, [FromBody] FreezeAccountRequest request)
    {
        var result = await _accountService.FreezeAccountAsync(id, request.Reason);
        return Ok(result);
    }
    
    [HttpGet("{id}/transactions")]
    public async Task<IActionResult> GetTransactions(Guid id, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var result = await _accountService.GetTransactionsAsync(id, from, to);
        return Ok(result);
    }
}