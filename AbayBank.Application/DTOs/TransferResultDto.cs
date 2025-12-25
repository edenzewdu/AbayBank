namespace AbayBank.Application.DTOs;

public class TransferResultDto
{
    public TransactionDto FromTransaction { get; set; } = null!;
    public TransactionDto ToTransaction { get; set; } = null!;
    public decimal NewBalance { get; set; }
}