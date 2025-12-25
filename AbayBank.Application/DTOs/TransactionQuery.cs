namespace AbayBank.Application.DTOs;

public class TransactionQuery
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? TransactionType { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}