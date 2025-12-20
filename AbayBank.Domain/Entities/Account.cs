using AbayBank.Domain.Enums;
using AbayBank.Domain.Exceptions;
using System.ComponentModel.DataAnnotations;

namespace AbayBank.Domain.Entities;

public class Account : BaseEntity
{
    [Required]
    public string AccountNumber { get; private set; }

    public decimal Balance { get; private set; }

    public AccountStatus Status { get; private set; }

    public AccountType AccountType { get; private set; }

    public Guid UserId { get; private set; }
    
    private readonly List<Transaction> _transactions = new();
    public IReadOnlyCollection<Transaction> Transactions => _transactions.AsReadOnly();

    // Private constructor for EF Core
    private Account() 
    {
        AccountNumber = string.Empty;
    }

    public Account(string accountNumber, Guid userId, AccountType accountType = AccountType.Savings, decimal initialBalance = 0)
    {
        Id = Guid.NewGuid();
        AccountNumber = accountNumber ?? throw new ArgumentNullException(nameof(accountNumber));
        UserId = userId;
        AccountType = accountType;
        Balance = initialBalance >= 0 ? initialBalance : throw new InvalidAccountOperationException("Initial balance cannot be negative.");
        Status = AccountStatus.Active;
        CreatedAt = DateTime.UtcNow;
    }

    public Transaction Deposit(decimal amount, string description = "Deposit")
    {
        if (Status != AccountStatus.Active)
            throw new InvalidAccountOperationException("Account is not active.");
        
        if (amount <= 0)
            throw new InvalidAccountOperationException("Deposit amount must be greater than zero.");

        Balance += amount;
        UpdatedAt = DateTime.UtcNow;
        
        var transaction = new Transaction(Id, TransactionType.Deposit.ToString(), amount, description);
        _transactions.Add(transaction);
        
        return transaction;
    }

    public Transaction Withdraw(decimal amount, string description = "Withdrawal")
    {
        if (Status != AccountStatus.Active)
            throw new InvalidAccountOperationException("Account is not active.");
        
        if (amount <= 0)
            throw new InvalidAccountOperationException("Withdrawal amount must be greater than zero.");

        if (amount > Balance)
            throw new InsufficientBalanceException();

        Balance -= amount;
        UpdatedAt = DateTime.UtcNow;
        
        var transaction = new Transaction(Id, TransactionType.Withdraw.ToString(), amount, description);
        _transactions.Add(transaction);
        
        return transaction;
    }

    public (Transaction fromTransaction, Transaction toTransaction) Transfer(
        Account toAccount, decimal amount, string description = "Transfer")
    {
        if (Status != AccountStatus.Active)
            throw new InvalidAccountOperationException("Source account is not active.");
        
        if (toAccount.Status != AccountStatus.Active)
            throw new InvalidAccountOperationException("Destination account is not active.");
        
        if (amount <= 0)
            throw new InvalidAccountOperationException("Transfer amount must be greater than zero.");
        
        if (amount > Balance)
            throw new InsufficientBalanceException();
        
        if (Id == toAccount.Id)
            throw new InvalidAccountOperationException("Cannot transfer to the same account.");

        // Withdraw from this account
        Balance -= amount;
        var fromTransaction = new Transaction(
            Id, 
            TransactionType.TransferOut.ToString(), 
            amount, 
            $"Transfer to {toAccount.AccountNumber}: {description}")
        {
            RelatedAccountId = toAccount.Id
        };
        _transactions.Add(fromTransaction);

        // Deposit to destination account
        toAccount.Balance += amount;
        var toTransaction = new Transaction(
            toAccount.Id,
            TransactionType.TransferIn.ToString(),
            amount,
            $"Transfer from {AccountNumber}: {description}")
        {
            RelatedAccountId = Id
        };
        toAccount._transactions.Add(toTransaction);

        UpdatedAt = DateTime.UtcNow;
        toAccount.UpdatedAt = DateTime.UtcNow;

        return (fromTransaction, toTransaction);
    }

    public void Freeze(string reason = "")
    {
        if (Status == AccountStatus.Closed)
            throw new InvalidAccountOperationException("Cannot freeze a closed account.");

        Status = AccountStatus.Frozen;
        UpdatedAt = DateTime.UtcNow;
        
        _transactions.Add(new Transaction(
            Id, 
            TransactionType.AccountFrozen.ToString(), 
            0, 
            $"Account frozen. Reason: {reason}"));
    }

    public void Unfreeze()
    {
        if (Status != AccountStatus.Frozen)
            throw new InvalidAccountOperationException("Account is not frozen.");

        Status = AccountStatus.Active;
        UpdatedAt = DateTime.UtcNow;
        
        _transactions.Add(new Transaction(
            Id, 
            TransactionType.AccountUnfrozen.ToString(), 
            0, 
            "Account unfrozen"));
    }

    public void Close()
    {
        if (Status == AccountStatus.Closed)
            throw new InvalidAccountOperationException("Account is already closed.");

        if (Balance > 0)
            throw new InvalidAccountOperationException("Account must have zero balance to close.");

        Status = AccountStatus.Closed;
        UpdatedAt = DateTime.UtcNow;
        
        _transactions.Add(new Transaction(
            Id, 
            TransactionType.AccountClosed.ToString(), 
            0, 
            "Account closed"));
    }

    public void UpdateAccountType(AccountType newType)
    {
        AccountType = newType;
        UpdatedAt = DateTime.UtcNow;
    }
}