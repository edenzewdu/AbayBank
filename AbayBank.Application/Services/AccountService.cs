using AbayBank.Application.DTOs;
using AbayBank.Application.Exceptions;
using AbayBank.Application.Interfaces;
using AbayBank.Domain.Entities;
using AbayBank.Domain.Enums;

namespace AbayBank.Application.Services;

public class AccountService
{
    private readonly IAccountRepository _accountRepository;
    private readonly ITransactionRepository _transactionRepository;

    public AccountService(
        IAccountRepository accountRepository,
        ITransactionRepository transactionRepository)
    {
        _accountRepository = accountRepository;
        _transactionRepository = transactionRepository;
    }

    public async Task<AccountResponse> CreateAccountAsync(CreateAccountRequest request)
    {
        var existing = await _accountRepository
            .GetByAccountNumberAsync(request.AccountNumber);

        if (existing != null)
            throw new AppException("Account already exists.");

        var account = new Account(
            request.AccountNumber,
            request.InitialBalance);

        await _accountRepository.AddAsync(account);

        if (request.InitialBalance > 0)
        {
            var transaction = new Transaction(
                account.Id,
                request.InitialBalance,
                TransactionType.Deposit);

            await _transactionRepository.AddAsync(transaction);
        }

        return MapToResponse(account);
    }

    public async Task<AccountResponse> DepositAsync(TransactionRequest request)
    {
        var account = await _accountRepository
            .GetByAccountNumberAsync(request.AccountNumber)
            ?? throw new NotFoundException("Account not found.");

        account.Deposit(request.Amount);

        await _accountRepository.UpdateAsync(account);

        var transaction = new Transaction(
            account.Id,
            request.Amount,
            TransactionType.Deposit);

        await _transactionRepository.AddAsync(transaction);

        return MapToResponse(account);
    }

    public async Task<AccountResponse> WithdrawAsync(TransactionRequest request)
    {
        var account = await _accountRepository
            .GetByAccountNumberAsync(request.AccountNumber)
            ?? throw new NotFoundException("Account not found.");

        account.Withdraw(request.Amount);

        await _accountRepository.UpdateAsync(account);

        var transaction = new Transaction(
            account.Id,
            request.Amount,
            TransactionType.Withdraw);

        await _transactionRepository.AddAsync(transaction);

        return MapToResponse(account);
    }

    private static AccountResponse MapToResponse(Account account)
    {
        return new AccountResponse
        {
            AccountNumber = account.AccountNumber,
            Balance = account.Balance,
            Status = account.Status
        };
    }
}
