using AbayBank.Application.DTOs;
using AbayBank.Application.Interfaces;
using AbayBank.Domain.Entities;
using AbayBank.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace AbayBank.Application.Services;

public class AccountService : IAccountService
{
    private readonly IAccountRepository _accountRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AccountService> _logger;


    public AccountService(
        IAccountRepository accountRepository,
        ITransactionRepository transactionRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ILogger<AccountService> logger)
    {
        _accountRepository = accountRepository;
        _transactionRepository = transactionRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<AccountDto> GetAccountByIdAsync(Guid id)
    {
        try
        {
            var account = await _accountRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Account with ID {id} not found.");
            
            return MapToDto(account);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting account by ID {Id}", id);
            throw;
        }
    }

    public async Task<AccountDto> GetAccountByNumberAsync(string accountNumber)
    {
        try
        {
            var account = await _accountRepository.GetByAccountNumberAsync(accountNumber)
                ?? throw new KeyNotFoundException($"Account {accountNumber} not found.");
            
            return MapToDto(account);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting account by number {AccountNumber}", accountNumber);
            throw;
        }
    }

    public async Task<IEnumerable<AccountDto>> GetUserAccountsAsync(Guid userId)
    {
        try
        {
            var accounts = await _accountRepository.GetByUserIdAsync(userId);
            return accounts.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user accounts for user {UserId}", userId);
            throw;
        }
    }
    public async Task<IEnumerable<AccountDto>> GetAllAccountsAsync()
    {
        try
        {
            var accounts = await _accountRepository.GetAllAsync();
            return accounts.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all accounts");
            throw;
        }
    }

    public async Task<AccountDto> CreateAccountAsync(CreateAccountRequest request, Guid userId)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId)
                ?? throw new KeyNotFoundException($"User with ID {userId} not found.");

            var existingAccount = await _accountRepository.GetByAccountNumberAsync(request.AccountNumber);
            if (existingAccount != null)
            {
                throw new ArgumentException($"Account number {request.AccountNumber} already exists.");
            }

            var account = new Account(
                request.AccountNumber,
                userId,
                request.AccountType,
                request.InitialBalance);

            await _accountRepository.AddAsync(account);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(account);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating account");
            throw;
        }
    }

    public async Task<AccountDto> UpdateAccountAsync(Guid id, UpdateAccountRequest request)
    {
        try
        {
            var account = await _accountRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Account with ID {id} not found.");

            if (request.AccountType.HasValue)
            {
                account.UpdateAccountType(request.AccountType.Value);
            }

            if (request.Status.HasValue)
            {
                switch (request.Status.Value)
                {
                    case Domain.Enums.AccountStatus.Active:
                        account.Unfreeze();
                        break;
                    case Domain.Enums.AccountStatus.Frozen:
                        account.Freeze("Updated via API");
                        break;
                    case Domain.Enums.AccountStatus.Closed:
                        account.Close();
                        break;
                }
            }

            await _accountRepository.UpdateAsync(account);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(account);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating account {Id}", id);
            throw;
        }
    }

    public async Task DeleteAccountAsync(Guid id)
    {
        try
        {
            var account = await _accountRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Account with ID {id} not found.");

            if (account.Balance > 0)
            {
                throw new InvalidOperationException("Cannot delete account with balance.");
            }

            await _accountRepository.DeleteAsync(account);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting account {Id}", id);
            throw;
        }
    }

    public async Task<TransactionDto> DepositAsync(DepositRequest request)
    {
        try
        {
            var account = await _accountRepository.GetByAccountNumberAsync(request.AccountNumber)
                ?? throw new KeyNotFoundException($"Account {request.AccountNumber} not found.");

            if (!ValidatePin(request.Pin))
            {
                throw new ArgumentException("Invalid PIN");
            }

            var tx = account.Deposit(request.Amount, request.Description ?? "Deposit");
            
            await _accountRepository.UpdateAsync(account);
            await _transactionRepository.AddAsync(tx);
            await _unitOfWork.SaveChangesAsync();

            return MapToTransactionDto(tx);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error depositing to account {AccountNumber}", request.AccountNumber);
            throw;
        }
    }

    public async Task<TransactionDto> WithdrawAsync(WithdrawRequest request)
    {
        try
        {
            var account = await _accountRepository.GetByAccountNumberAsync(request.AccountNumber)
                ?? throw new KeyNotFoundException($"Account {request.AccountNumber} not found.");

            if (!ValidatePin(request.Pin))
            {
                throw new ArgumentException("Invalid PIN");
            }

            var tx = account.Withdraw(request.Amount, request.Description ?? "Withdrawal");
            
            await _accountRepository.UpdateAsync(account);
            await _transactionRepository.AddAsync(tx);
            await _unitOfWork.SaveChangesAsync();

            return MapToTransactionDto(tx);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error withdrawing from account {AccountNumber}", request.AccountNumber);
            throw;
        }
    }

    public async Task<TransferResultDto> TransferAsync(TransferRequest request)
    {
        try
        {
            var fromAccount = await _accountRepository.GetByIdAsync(request.FromAccountId)
                ?? throw new KeyNotFoundException($"Source account not found.");

            var toAccount = await _accountRepository.GetByAccountNumberAsync(request.ToAccountNumber)
                ?? throw new KeyNotFoundException($"Destination account {request.ToAccountNumber} not found.");

            if (!ValidatePin(request.Pin))
            {
                throw new ArgumentException("Invalid PIN");
            }

            var (fromTransaction, toTransaction) = fromAccount.Transfer(
                toAccount, request.Amount, request.Description);
            
            await _accountRepository.UpdateAsync(fromAccount);
            await _accountRepository.UpdateAsync(toAccount);
            await _transactionRepository.AddAsync(fromTransaction);
            await _transactionRepository.AddAsync(toTransaction);
            await _unitOfWork.SaveChangesAsync();

            return new TransferResultDto
            {
                FromTransaction = MapToTransactionDto(fromTransaction),
                ToTransaction = MapToTransactionDto(toTransaction),
                NewBalance = fromAccount.Balance
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transferring from account {FromAccountId} to {ToAccountNumber}", 
                request.FromAccountId, request.ToAccountNumber);
            throw;
        }
    }

    public async Task<IEnumerable<TransactionDto>> GetTransactionsAsync(Guid accountId, DateTime? from, DateTime? to)
    {
        try
        {
            var account = await _accountRepository.GetByIdAsync(accountId)
                ?? throw new KeyNotFoundException($"Account not found.");

            var transactions = await _transactionRepository.GetByAccountIdAsync(
                accountId, 
                from ?? DateTime.UtcNow.AddDays(-30),
                to ?? DateTime.UtcNow);

            return transactions.Select(MapToTransactionDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transactions for account {AccountId}", accountId);
            throw;
        }
    }

    public async Task<IEnumerable<TransactionDto>> GetAccountTransactionsAsync(Guid accountId, TransactionQuery query)
    {
        try
        {
            var account = await _accountRepository.GetByIdAsync(accountId)
                ?? throw new KeyNotFoundException($"Account not found.");

            var transactions = await _transactionRepository.GetByAccountIdWithPagingAsync(
                accountId, 
                query.FromDate ?? DateTime.UtcNow.AddDays(-30),
                query.ToDate ?? DateTime.UtcNow,
                query.TransactionType,
                query.Page,
                query.PageSize);

            return transactions.Select(MapToTransactionDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transactions for account {AccountId}", accountId);
            throw;
        }
    }

    public async Task<AccountDto> FreezeAccountAsync(Guid accountId, string reason)
    {
        try
        {
            var account = await _accountRepository.GetByIdAsync(accountId)
                ?? throw new KeyNotFoundException($"Account not found.");

            account.Freeze(reason);
            await _accountRepository.UpdateAsync(account);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(account);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error freezing account {AccountId}", accountId);
            throw;
        }
    }

    public async Task<AccountDto> UnfreezeAccountAsync(Guid accountId)
    {
        try
        {
            var account = await _accountRepository.GetByIdAsync(accountId)
                ?? throw new KeyNotFoundException($"Account not found.");

            account.Unfreeze();
            await _accountRepository.UpdateAsync(account);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(account);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unfreezing account {AccountId}", accountId);
            throw;
        }
    }

    private bool ValidatePin(string? pin)
    {
        return !string.IsNullOrEmpty(pin) && 
               pin.Length == 4 && 
               pin.All(char.IsDigit);
    }

    private AccountDto MapToDto(Account account)
    {
        return new AccountDto
        {
            Id = account.Id,
            AccountNumber = account.AccountNumber,
            Balance = account.Balance,
            Status = account.Status.ToString(),
            AccountType = account.AccountType.ToString(),
            UserId = account.UserId,
            CreatedAt = account.CreatedAt,
            UpdatedAt = account.UpdatedAt
        };
    }

    private TransactionDto MapToTransactionDto(Transaction transaction)
    {
        return new TransactionDto
        {
            Id = transaction.Id,
            AccountId = transaction.AccountId,
            RelatedAccountId = transaction.RelatedAccountId,
            TransactionType = transaction.TransactionType,
            Amount = transaction.Amount,
            Description = transaction.Description,
            Timestamp = transaction.Timestamp,
            ReferenceNumber = transaction.ReferenceNumber,
            Status = transaction.Status
        };
    }
}