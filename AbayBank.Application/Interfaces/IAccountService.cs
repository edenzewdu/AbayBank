using AbayBank.Application.DTOs;

namespace AbayBank.Application.Interfaces;

public interface IAccountService
{
    Task<AccountDto> GetAccountByIdAsync(Guid id);
    Task<AccountDto> GetAccountByNumberAsync(string accountNumber);
    Task<IEnumerable<AccountDto>> GetUserAccountsAsync(Guid userId);
    
    Task<AccountDto> CreateAccountAsync(CreateAccountRequest request, Guid userId);
    Task<AccountDto> UpdateAccountAsync(Guid id, UpdateAccountRequest request);
    Task DeleteAccountAsync(Guid id);
    
    Task<TransactionDto> DepositAsync(DepositRequest request);
    Task<TransactionDto> WithdrawAsync(WithdrawRequest request);
    Task<TransferResultDto> TransferAsync(TransferRequest request);
    
    Task<IEnumerable<TransactionDto>> GetTransactionsAsync(Guid accountId, DateTime? from, DateTime? to);
    Task<IEnumerable<TransactionDto>> GetAccountTransactionsAsync(Guid accountId, TransactionQuery query);
    
    Task<AccountDto> FreezeAccountAsync(Guid accountId, string reason);
    Task<AccountDto> UnfreezeAccountAsync(Guid accountId);
}