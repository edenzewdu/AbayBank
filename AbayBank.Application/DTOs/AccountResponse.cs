using AbayBank.Domain.Enums;

namespace AbayBank.Application.DTOs;

public class AccountResponse
{
    public string AccountNumber { get; set; } = default!;
    public decimal Balance { get; set; }
    public AccountStatus Status { get; set; }
}
