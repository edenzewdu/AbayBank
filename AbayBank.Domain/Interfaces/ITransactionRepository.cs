using AbayBank.Domain.Entities;

namespace AbayBank.Domain.Interfaces;

public interface ITransactionRepository
{
    // Read operations
    Task<Transaction?> GetByIdAsync(Guid id);
    Task<IEnumerable<Transaction>> GetByAccountIdAsync(Guid accountId, DateTime from, DateTime to);
    Task<IEnumerable<Transaction>> GetByAccountIdAndTypeAsync(Guid accountId, string transactionType, DateTime from, DateTime to);
    Task<IEnumerable<Transaction>> GetByAccountIdWithPagingAsync(
        Guid accountId, 
        DateTime from, 
        DateTime to, 
        string? transactionType,
        int page,
        int pageSize);
    Task<int> CountByAccountIdAsync(Guid accountId, DateTime? from = null, DateTime? to = null);

    // Create operations
    Task AddAsync(Transaction transaction);
    Task AddRangeAsync(IEnumerable<Transaction> transactions);
}