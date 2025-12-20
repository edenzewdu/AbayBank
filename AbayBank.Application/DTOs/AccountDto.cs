using AbayBank.Domain.Enums;

namespace AbayBank.Application.DTOs;

public class AccountDto
{
    public Guid Id { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public AccountStatus Status { get; set; }
    public AccountType AccountType { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateAccountRequest
{
    public string AccountNumber { get; set; } = string.Empty;
    public AccountType AccountType { get; set; } = AccountType.Savings;
    public decimal InitialBalance { get; set; } = 0;
}

public class UpdateAccountRequest
{
    public AccountType? AccountType { get; set; }
    public AccountStatus? Status { get; set; }
}

public class DepositRequest
{
    public string AccountNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Pin { get; set; }
}

public class WithdrawRequest
{
    public string AccountNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Pin { get; set; }
}

public class TransferRequest
{
    public Guid FromAccountId { get; set; }
    public string ToAccountNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Pin { get; set; }
}

public class TransferResultDto
{
    public TransactionDto FromTransaction { get; set; } = null!;
    public TransactionDto ToTransaction { get; set; } = null!;
    public decimal NewBalance { get; set; }
}

public class TransactionDto
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public Guid? RelatedAccountId { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class TransactionQuery
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? TransactionType { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class FreezeAccountRequest
{
    public string Reason { get; set; } = string.Empty;
}