using AbayBank.Domain.Entities;

namespace AbayBank.Domain.Interfaces;

public interface IAccountRepository
{
    // Read operations
    Task<Account?> GetByIdAsync(Guid id);
    Task<Account?> GetByAccountNumberAsync(string accountNumber);
    Task<IEnumerable<Account>> GetByUserIdAsync(Guid userId);
    Task<bool> ExistsAsync(string accountNumber);

    // Create operations
    Task AddAsync(Account account);

    // Update operations
    Task UpdateAsync(Account account);

    // Delete operations
    Task DeleteAsync(Account account);
}