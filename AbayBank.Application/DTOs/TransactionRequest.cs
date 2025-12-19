namespace AbayBank.Application.DTOs;

public class TransactionRequest
{
    public string AccountNumber { get; set; } = default!;
    public decimal Amount { get; set; }
}
