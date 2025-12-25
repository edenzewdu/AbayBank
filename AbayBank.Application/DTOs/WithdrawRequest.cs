namespace AbayBank.Application.DTOs;

public class WithdrawRequest
{
    public string AccountNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Pin { get; set; }
}