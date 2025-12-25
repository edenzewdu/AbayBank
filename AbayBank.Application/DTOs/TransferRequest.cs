namespace AbayBank.Application.DTOs;

public class TransferRequest
{
    public Guid FromAccountId { get; set; }
    public string ToAccountNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Pin { get; set; }
}