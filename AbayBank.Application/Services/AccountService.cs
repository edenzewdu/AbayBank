using AbayBank.Application.DTOs;
using AbayBank.Application.Exceptions;
using AbayBank.Application.Interfaces;
using AbayBank.Domain.Entities;
using AbayBank.Domain.Enums;
using AbayBank.Domain.Interfaces;

namespace AbayBank.Application.Services;

public class AccountService : IAccountService
{
    private readonly IAccountRepository _accountRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AccountService(
        IAccountRepository accountRepository,
        ITransactionRepository transactionRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _accountRepository = accountRepository;
        _transactionRepository = transactionRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AccountDto> GetAccountByIdAsync(Guid id)
    {
        var account = await _accountRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Account with ID {id} not found.");
        
        return MapToDto(account);
    }

    public async Task<AccountDto> GetAccountByNumberAsync(string accountNumber)
    {
        var account = await _accountRepository.GetByAccountNumberAsync(accountNumber)
            ?? throw new NotFoundException($"Account {accountNumber} not found.");
        
        return MapToDto(account);
    }

    public async Task<IEnumerable<AccountDto>> GetUserAccountsAsync(Guid userId)
    {
        var accounts = await _accountRepository.GetByUserIdAsync(userId);
        return accounts.Select(MapToDto);
    }

    public async Task<AccountDto> CreateAccountAsync(CreateAccountRequest request, Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new NotFoundException($"User with ID {userId} not found.");

        var existingAccount = await _accountRepository.GetByAccountNumberAsync(request.AccountNumber);
        if (existingAccount != null)
        {
            throw new AppException($"Account number {request.AccountNumber} already exists.");
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

    public async Task<AccountDto> UpdateAccountAsync(Guid id, UpdateAccountRequest request)
    {
        var account = await _accountRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Account with ID {id} not found.");

        if (request.AccountType.HasValue)
        {
            account.UpdateAccountType(request.AccountType.Value);
        }

        await _accountRepository.UpdateAsync(account);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(account);
    }

    public async Task DeleteAccountAsync(Guid id)
    {
        var account = await _accountRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Account with ID {id} not found.");

        if (account.Balance > 0)
        {
            throw new AppException("Cannot delete account with balance.");
        }

        await _accountRepository.DeleteAsync(account);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<TransactionDto> DepositAsync(DepositRequest request)
    {
        var account = await _accountRepository.GetByAccountNumberAsync(request.AccountNumber)
            ?? throw new NotFoundException($"Account {request.AccountNumber} not found.");

        if (!ValidatePin(request.Pin))
        {
            throw new AppException("Invalid PIN");
        }

        var transaction = account.Deposit(request.Amount, request.Description);
        
        await _accountRepository.UpdateAsync(account);
        await _transactionRepository.AddAsync(transaction);
        await _unitOfWork.SaveChangesAsync();

        return MapToTransactionDto(transaction);
    }

    public async Task<TransactionDto> WithdrawAsync(WithdrawRequest request)
    {
        var account = await _accountRepository.GetByAccountNumberAsync(request.AccountNumber)
            ?? throw new NotFoundException($"Account {request.AccountNumber} not found.");

        if (!ValidatePin(request.Pin))
        {
            throw new AppException("Invalid PIN");
        }

        var transaction = account.Withdraw(request.Amount, request.Description);
        
        await _accountRepository.UpdateAsync(account);
        await _transactionRepository.AddAsync(transaction);
        await _unitOfWork.SaveChangesAsync();

        return MapToTransactionDto(transaction);
    }

    public async Task<TransferResultDto> TransferAsync(TransferRequest request)
    {
        var fromAccount = await _accountRepository.GetByIdAsync(request.FromAccountId)
            ?? throw new NotFoundException($"Source account not found.");

        var toAccount = await _accountRepository.GetByAccountNumberAsync(request.ToAccountNumber)
            ?? throw new NotFoundException($"Destination account {request.ToAccountNumber} not found.");

        if (!ValidatePin(request.Pin))
        {
            throw new AppException("Invalid PIN");
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

    public async Task<IEnumerable<TransactionDto>> GetTransactionsAsync(Guid accountId, DateTime? from, DateTime? to)
    {
        var account = await _accountRepository.GetByIdAsync(accountId)
            ?? throw new NotFoundException($"Account not found.");

        var transactions = await _transactionRepository.GetByAccountIdAsync(
            accountId, 
            from ?? DateTime.UtcNow.AddDays(-30),
            to ?? DateTime.UtcNow);

        return transactions.Select(MapToTransactionDto);
    }

    public async Task<IEnumerable<TransactionDto>> GetAccountTransactionsAsync(Guid accountId, TransactionQuery query)
    {
        var account = await _accountRepository.GetByIdAsync(accountId)
            ?? throw new NotFoundException($"Account not found.");

        var transactions = await _transactionRepository.GetByAccountIdWithPagingAsync(
            accountId, 
            query.FromDate ?? DateTime.UtcNow.AddDays(-30),
            query.ToDate ?? DateTime.UtcNow,
            query.TransactionType,
            query.Page,
            query.PageSize);

        return transactions.Select(MapToTransactionDto);
    }

    public async Task<AccountDto> FreezeAccountAsync(Guid accountId, string reason)
    {
        var account = await _accountRepository.GetByIdAsync(accountId)
            ?? throw new NotFoundException($"Account not found.");

        account.Freeze(reason);
        await _accountRepository.UpdateAsync(account);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(account);
    }

    public async Task<AccountDto> UnfreezeAccountAsync(Guid accountId)
    {
        var account = await _accountRepository.GetByIdAsync(accountId)
            ?? throw new NotFoundException($"Account not found.");

        account.Unfreeze();
        await _accountRepository.UpdateAsync(account);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(account);
    }

    private bool ValidatePin(string? pin)
    {
        return !string.IsNullOrEmpty(pin) && pin.Length == 4 && pin.All(char.IsDigit);
    }

    private AccountDto MapToDto(Account account)
    {
        return new AccountDto
        {
            Id = account.Id,
            AccountNumber = account.AccountNumber,
            Balance = account.Balance,
            Status = account.Status,
            AccountType = account.AccountType,
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