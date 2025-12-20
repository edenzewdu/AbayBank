using AbayBank.Domain.Entities;

namespace AbayBank.Domain.Interfaces;

public interface IUserRepository
{
    // Read operations
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task<IEnumerable<User>> GetAllAsync();
    Task<bool> ExistsByEmailAsync(string email);

    // Create operations
    Task AddAsync(User user);

    // Update operations
    Task UpdateAsync(User user);

    // Delete operations
    Task DeleteAsync(User user);

    Task<int> SaveChangesAsync();
}