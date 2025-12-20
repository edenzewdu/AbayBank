using AbayBank.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace AbayBank.Domain.Entities;

public class Transaction : BaseEntity
{
    public Guid AccountId { get; private set; }
    public Guid? RelatedAccountId { get; set; }

    [Required]
    public string TransactionType { get; private set; }

    public decimal Amount { get; private set; }

    public string Description { get; private set; }

    public DateTime Timestamp { get; private set; }

    [Required]
    public string ReferenceNumber { get; private set; }

    [Required]
    public string Status { get; private set; }

    // Private constructor for EF Core
    private Transaction() 
    {
        TransactionType = string.Empty;
        Description = string.Empty;
        ReferenceNumber = string.Empty;
        Status = "Completed";
    }

    // Constructor that accepts string
    public Transaction(Guid accountId, string transactionType, decimal amount, string description)
    {
        Id = Guid.NewGuid();
        AccountId = accountId;
        TransactionType = transactionType ?? throw new ArgumentNullException(nameof(transactionType));
        Amount = amount;
        Description = description ?? string.Empty;
        Timestamp = DateTime.UtcNow;
        ReferenceNumber = GenerateReferenceNumber();
        Status = "Completed";
        CreatedAt = DateTime.UtcNow;
    }

    private static string GenerateReferenceNumber()
    {
        return $"TX-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";
    }

    public void UpdateStatus(string newStatus)
    {
        Status = newStatus ?? throw new ArgumentNullException(nameof(newStatus));
        UpdatedAt = DateTime.UtcNow;
    }
}