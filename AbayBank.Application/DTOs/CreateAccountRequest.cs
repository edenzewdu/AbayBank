namespace AbayBank.Application.DTOs;

public class CreateAccountRequest
{
    public string AccountNumber { get; set; } = default!;
    public decimal InitialBalance { get; set; }
}
