using AbayBank.Domain.Enums;
using AbayBank.Domain.Exceptions;
using System.Collections.Generic;

namespace AbayBank.Domain.Entities;

public class Account
{
    public Guid Id { get; private set; }
    public string AccountNumber { get; private set; }
    public decimal Balance { get; private set; }
    public AccountStatus Status { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    private readonly List<Transaction> _transactions = new List<Transaction>();
    public IReadOnlyCollection<Transaction> Transactions => _transactions.AsReadOnly();

    private Account() { }

    public Account(string accountNumber, Guid userId, decimal initialBalance = 0)
    {
        if (string.IsNullOrWhiteSpace(accountNumber))
            throw new InvalidAccountOperationException("Account number is required.");

        if (initialBalance < 0)
            throw new InvalidAccountOperationException("Initial balance cannot be negative.");

        Id = Guid.NewGuid();
        AccountNumber = accountNumber;
        UserId = userId;
        Balance = initialBalance;
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
        
        var transaction = new Transaction(Id, null, "Deposit", amount, description);
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
        
        var transaction = new Transaction(Id, null, "Withdraw", amount, description);
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
        var fromTransaction = new Transaction(Id, toAccount.Id, "Transfer_Out", amount, 
                                            $"Transfer to {toAccount.AccountNumber}: {description}");

        // Deposit to destination account
        toAccount.Balance += amount;
        var toTransaction = new Transaction(toAccount.Id, Id, "Transfer_In", amount, 
                                          $"Transfer from {AccountNumber}: {description}");

        _transactions.Add(fromTransaction);
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
        
        _transactions.Add(new Transaction(Id, null, "Account_Frozen", 0, 
                                        $"Account frozen. Reason: {reason}"));
    }

    public void Unfreeze()
    {
        if (Status != AccountStatus.Frozen)
            throw new InvalidAccountOperationException("Account is not frozen.");

        Status = AccountStatus.Active;
        UpdatedAt = DateTime.UtcNow;
        
        _transactions.Add(new Transaction(Id, null, "Account_Unfrozen", 0, "Account unfrozen"));
    }

    public void Close()
    {
        if (Status == AccountStatus.Closed)
            throw new InvalidAccountOperationException("Account is already closed.");

        if (Balance > 0)
            throw new InvalidAccountOperationException("Account must have zero balance to close.");

        Status = AccountStatus.Closed;
        UpdatedAt = DateTime.UtcNow;
        
        _transactions.Add(new Transaction(Id, null, "Account_Closed", 0, "Account closed"));
    }
}