using AbayBank.Domain.Enums;

namespace AbayBank.Application.DTOs;

public class UpdateAccountRequest
{
    public AccountType? AccountType { get; set; }
    public AccountStatus? Status { get; set; }
}