using AbayBank.Domain.Enums;

namespace AbayBank.Application.DTOs;

public class CreateAccountRequest
{
    public string AccountNumber { get; set; } = string.Empty;
    public AccountType AccountType { get; set; } = AccountType.Savings;
    public decimal InitialBalance { get; set; } = 0;
}