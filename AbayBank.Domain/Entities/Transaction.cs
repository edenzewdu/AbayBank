namespace AbayBank.Domain.Entities;

public class Transaction
{
    public Guid Id { get; private set; }
    public Guid FromAccountId { get; private set; }
    public Guid? ToAccountId { get; private set; }
    public string TransactionType { get; private set; } // Deposit, Withdraw, Transfer
    public decimal Amount { get; private set; }
    public string Description { get; private set; }
    public DateTime Timestamp { get; private set; }
    public string ReferenceNumber { get; private set; }

    private Transaction() { }

    public Transaction(Guid fromAccountId, Guid? toAccountId, string transactionType, 
                      decimal amount, string description)
    {
        Id = Guid.NewGuid();
        FromAccountId = fromAccountId;
        ToAccountId = toAccountId;
        TransactionType = transactionType;
        Amount = amount;
        Description = description;
        Timestamp = DateTime.UtcNow;
        ReferenceNumber = $"TX-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
    }
}
